using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record GetChatDetailsQuery(Guid ContractId) : IRequest<Result<ChatDetailsVM>>;

public class GetChatDetailsQueryHandler(
    IContractQueries contractQueries,
    IUserProvider userProvider) : IRequestHandler<GetChatDetailsQuery, Result<ChatDetailsVM>>
{
    public async Task<Result<ChatDetailsVM>> Handle(GetChatDetailsQuery request, CancellationToken cancellationToken)
    {
        var contract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken);
        if (contract == null)
            return Result<ChatDetailsVM>.NotFound("Contract not found");

        var currentUserId = await userProvider.GetUserId();

        if (contract.FreelancerId != currentUserId && contract.CreatedBy != currentUserId)
            return Result<ChatDetailsVM>.Forbidden("You are not a participant of this contract");

        var isFreelancer = currentUserId == contract.FreelancerId;
        
        // В залежності від того, хто запитує, співрозмовником є інша сторона
        var interlocutorId = isFreelancer ? contract.CreatedBy : contract.FreelancerId;
        // Project navigation property should be loaded in GetByIdAsync ideally. 
        // If not, we will just use basic info.
        
        var vm = new ChatDetailsVM
        {
            ContractId = contract.Id,
            ProjectTitle = contract.Project?.Title ?? "Project",
            InterlocutorId = interlocutorId,
            InterlocutorName = isFreelancer ? "Employer" : "Freelancer",
            InterlocutorAvatar = isFreelancer ? null : contract.Freelancer?.AvatarLogo, // For real app, need User entity included
            ContractStatus = contract.Status.ToString()
        };

        return Result<ChatDetailsVM>.Ok("Chat details retrieved", vm);
    }
}
