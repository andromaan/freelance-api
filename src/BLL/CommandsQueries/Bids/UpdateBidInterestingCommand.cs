using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.Services.Notifications;
using BLL.ViewModels.Bid;
using Domain.Models.Notifications;
using MediatR;

namespace BLL.CommandsQueries.Bids;

public record UpdateBidInterestingCommand : IRequest<ServiceResponse>
{
    public required Guid BidId { get; init; }
    public required bool IsInteresting { get; init; }
}

public class UpdateBidInterestingCommandHandler(
    IBidQueries bidQueries,
    IBidRepository bidRepository,
    IUserProvider userProvider,
    IProjectQueries projectQueries,
    INotificationService notificationService,
    IMapper mapper) : IRequestHandler<UpdateBidInterestingCommand, ServiceResponse>
{
    public async Task<ServiceResponse> Handle(UpdateBidInterestingCommand request, CancellationToken cancellationToken)
    {
        var existingEntity = await bidQueries.GetByIdAsync(request.BidId, cancellationToken);

        if (existingEntity == null)
        {
            return ServiceResponse.NotFound($"Bid with Id {request.BidId} not found");
        }

        var project = await projectQueries.GetByIdAsync(existingEntity.ProjectId, cancellationToken);

        if (project is null)
        {
            return ServiceResponse.NotFound($"Project with Id {existingEntity.ProjectId} not found");
        }

        var userId = await userProvider.GetUserId(cancellationToken);

        if (project.CreatedBy != userId)
        {
            return ServiceResponse.Forbidden("Only the project owner can update the bid's interesting status.");
        }

        existingEntity.IsInteresting = request.IsInteresting;

        await notificationService.SendAsync(
            message: request.IsInteresting
                ? $"Your bid for project \"{project.Title}\" is interesting to employer."
                : $"Your bid for project \"{project.Title}\" is NOT interesting to employer.",
            type: request.IsInteresting
                ? NotificationType.InterestedInYourBid
                : NotificationType.NotInterestedInYourBid,
            userId: existingEntity.CreatedBy,
            cancellationToken: cancellationToken,
            linkAddress: "/profile/my-bids");

        try
        {
            await bidRepository.UpdateAsync(existingEntity, cancellationToken);
            return ServiceResponse.Ok("Bid interesting status updated",
                mapper.Map<BidVM>(existingEntity));
        }
        catch (Exception e)
        {
            return ServiceResponse.InternalError(e.Message);
        }
    }
}