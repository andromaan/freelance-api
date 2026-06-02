using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record GetChatDetailsQuery(Guid ContractId) : IRequest<Result<ChatDetailsVM>>;

public class GetChatDetailsQueryHandler(
    IContractQueries contractQueries,
    IUserProvider userProvider,
    IFreelancerQueries freelancerQueries,
    IUserQueries userQueries,
    IProjectQueries projectQueries,
    Hubs.ChatPresenceTracker presenceTracker) : IRequestHandler<GetChatDetailsQuery, Result<ChatDetailsVM>>
{
    public async Task<Result<ChatDetailsVM>> Handle(GetChatDetailsQuery request, CancellationToken cancellationToken)
    {
        var contract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken);
        if (contract == null)
            return Result<ChatDetailsVM>.NotFound("Contract not found");

        var currentUserId = await userProvider.GetUserId(cancellationToken);

        var freelancer = await freelancerQueries.GetByIdAsync(contract.FreelancerId, cancellationToken);

        if (contract.FreelancerId != freelancer!.Id && contract.CreatedBy != currentUserId)
            return Result<ChatDetailsVM>.Forbidden("You are not a participant of this contract");

        var isFreelancer = currentUserId == freelancer.CreatedBy;

        // В залежності від того, хто запитує, співрозмовником є інша сторона
        var interlocutorId = isFreelancer ? contract.CreatedBy : freelancer.CreatedBy;
        var interlocutorUser = await userQueries.GetByIdAsync(interlocutorId, cancellationToken);
        // Project navigation property should be loaded in GetByIdAsync ideally. 
        // If not, we will just use basic info.

        var project = await projectQueries.GetByIdAsync(contract.ProjectId, cancellationToken);

        var vm = new ChatDetailsVM
        {
            ContractId = contract.Id,
            ProjectTitle = project?.Title ?? "Project",
            InterlocutorId = interlocutorId,
            InterlocutorName = interlocutorUser!.DisplayName ?? interlocutorUser.Email,
            InterlocutorAvatar = interlocutorUser.AvatarImg,
            ContractStatus = contract.Status.ToString(),
            IsInterlocutorOnline = await presenceTracker.IsUserOnline(interlocutorId),
            InterlocutorRole = interlocutorUser.Role!.Name
        };

        return Result<ChatDetailsVM>.Ok("Chat details retrieved", vm);
    }
}