using AutoMapper;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.ProjectMilestones;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using BLL.Services.Notifications;
using BLL.ViewModels.Contract;
using Domain.Models.Contracts;
using Domain.Models.Notifications;
using Domain.Models.Projects;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public class CreateContractCommand : IRequest<Result<ContractVM?>>
{
    public required Guid QuoteId { get; init; }
}

public class CreateContractCommandHandler(
    IContractRepository contractRepository,
    IContractQueries contractQueries,
    IQuoteQueries quoteQueries,
    IProjectQueries projectQueries,
    IMapper mapper,
    IProjectMilestoneQueries projectMilestoneQueries,
    IContractMilestoneRepository contractMilestoneRepository,
    IProjectRepository projectRepository,
    IBidQueries bidQueries,
    INotificationService notificationService,
    IFreelancerQueries freelancerQueries)
    : IRequestHandler<CreateContractCommand, Result<ContractVM?>>
{
    public async Task<Result<ContractVM?>> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var quote = await quoteQueries.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Result<ContractVM?>.NotFound($"Quote with id {request.QuoteId} not found");
        }

        var project = await projectQueries.GetByIdAsync(quote.ProjectId, cancellationToken);
        var projectMilestones
            = (await projectMilestoneQueries.GetByProjectIdAsync(quote.ProjectId, cancellationToken)).ToList();

        if (projectMilestones.Count == 0)
        {
            return Result<ContractVM?>.BadRequest(
                $"Contract cannot be created. Project with id {quote.ProjectId} has no milestones.");
        }

        if (!await contractQueries.IsContractCanBeCreated(project!.Id, project.CreatedBy, quote.FreelancerId,
                cancellationToken))
        {
            return Result<ContractVM?>.InternalError(
                "Contract cannot be created. Contract already exists for this quote.");
        }

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            FreelancerId = quote.FreelancerId,
            StartDate = DateTime.UtcNow,
            EndDate = projectMilestones.Any()
                ? projectMilestones.OrderByDescending(x => x.DueDate).First().DueDate
                : project.Deadline,
            AgreedRate = quote.Amount,
            Status = ContractStatus.Pending,
        };

        try
        {
            var createdEntity = await contractRepository.CreateAsync(contract, cancellationToken);

            foreach (var pMilestone in projectMilestones)
            {
                var cMilestone = new ContractMilestone
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Description = pMilestone.Description,
                    DueDate = pMilestone.DueDate,
                    Amount = pMilestone.Amount,
                    Status = ContractMilestoneStatus.Pending
                };

                await contractMilestoneRepository.CreateAsync(cMilestone, cancellationToken);
            }

            // Update project status to InProgress
            var projectChangeStatusResult = await UpdateStatusAsync(quote.ProjectId, cancellationToken);
            if (projectChangeStatusResult != null)
                return projectChangeStatusResult;

            await SendNotificationsToFreelancers(quote, project, cancellationToken);

            return Result<ContractVM?>.Ok($"Contract created",
                mapper.Map<ContractVM>(createdEntity));
        }
        catch (Exception exception)
        {
            return Result<ContractVM?>.InternalError(exception.Message);
        }
    }

    private async Task SendNotificationsToFreelancers(Quote quote, Project project,
        CancellationToken cancellationToken)
    {
        var bids = await bidQueries.GetByProjectIdAsync(project.Id, cancellationToken);
        var freelancerIdsToNotifyAboutTakenProject =
            bids.Select(b => b.FreelancerId).Where(id => id != quote.FreelancerId).ToList();

        foreach (var freelancerId in freelancerIdsToNotifyAboutTakenProject)
        {
            var freelancerUser = await freelancerQueries.GetByIdAsync(freelancerId, cancellationToken);

            if (freelancerUser is null)
                continue;

            await notificationService.SendAsync($"Project '{project.Title}' has been taken by another freelancer.",
                NotificationType.ProposalRejected, freelancerUser.CreatedBy, cancellationToken);
        }

        var freelancer = await freelancerQueries.GetByIdAsync(quote.FreelancerId, cancellationToken);

        if (freelancer is null)
            throw new Exception($"Freelancer with id {quote.FreelancerId} not found");

        await notificationService.SendAsync($"Your quote for project '{project.Title}' has been accepted.",
            NotificationType.ProposalAccepted, freelancer.CreatedBy, cancellationToken);
    }

    private async Task<Result<ContractVM?>?> UpdateStatusAsync(Guid quoteProjectId, CancellationToken cancellationToken)
    {
        var project = await projectQueries.GetByIdAsync(quoteProjectId, cancellationToken);
        if (project is null)
        {
            return Result<ContractVM?>.NotFound($"Project with id {quoteProjectId} not found");
        }

        project.Status = ProjectStatus.InProgress;

        try
        {
            await projectRepository.UpdateAsync(project, cancellationToken);
        }
        catch (Exception e)
        {
            return Result<ContractVM?>.InternalError(e.Message);
        }

        return null;
    }
}