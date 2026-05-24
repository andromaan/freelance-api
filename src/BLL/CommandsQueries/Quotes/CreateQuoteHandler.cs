using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Employers;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using BLL.Services.Notifications;
using BLL.ViewModels.Quote;
using Domain.Models.Notifications;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.Quotes;

/// <summary>
/// Unified handler for Quote creation that combines validation and processing.
/// Replaces CreateQuoteValidator + CreateQuoteProcessor.
/// </summary>
public class CreateQuoteHandler(
    IProjectQueries projectQueries,
    IUserProvider userProvider,
    IFreelancerQueries freelancerQueries,
    IQuoteQueries quoteQueries,
    INotificationService notificationService,
    IEmployerQueries employerQueries)
    : ICreateHandler<Quote, CreateQuoteVM, QuoteVM>
{
    public async Task<Result<QuoteVM?>> HandleAsync(
        Quote entity,
        CreateQuoteVM createModel,
        CancellationToken cancellationToken)
    {
        // Validation: Check if project exists
        var existingProject = await projectQueries.GetByIdAsync(createModel.ProjectId, cancellationToken);

        if (existingProject is null)
        {
            return Result<QuoteVM?>.NotFound($"Project with Id {createModel.ProjectId} not found");
        }

        // Processing: Set FreelancerId from current user
        var userId = await userProvider.GetUserId(cancellationToken);
        var existingFreelancer = await freelancerQueries.GetByUserIdAsync(userId, cancellationToken);

        if (existingFreelancer is null)
        {
            return Result<QuoteVM?>.NotFound("Freelancer not found for current user");
        }
        
        entity.FreelancerId = existingFreelancer.Id;
        
        var quotesByProject = await quoteQueries.GetByProjectIdAsync(createModel.ProjectId, cancellationToken);
        if (quotesByProject.Any(b => b.FreelancerId == entity.FreelancerId))
        {
            return Result<QuoteVM?>.BadRequest("You have already placed a quote on this project");
        }

        if (existingProject.Budget < createModel.Amount)
        {
            return Result<QuoteVM?>.BadRequest(
                $"Quote amount {createModel.Amount} exceeds project budget {existingProject.Budget}");
        }
        
        // Notify: Find employer (owner of the project) and send notification
        var employer = await employerQueries.GetByUserId(existingProject.CreatedBy, cancellationToken);
        if (employer is not null)
        {
            await notificationService.SendAsync(
                message: $"You received a new quote of {createModel.Amount:C} on your project \"{existingProject.Title}\".",
                type: NotificationType.NewQuoteReceived,
                userId: existingProject.CreatedBy,
                cancellationToken: cancellationToken,
                linkAddress: $"/my-projects/{existingProject.Id}/quotes");
        }

        // Return success with processed entity
        return Result<QuoteVM?>.Ok();
    }
}