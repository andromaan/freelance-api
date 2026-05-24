using BLL.Common.Handlers;
using BLL.Services;
using BLL.ViewModels.Dispute;
using Domain.Models.Disputes;

namespace BLL.CommandsQueries.Disputes.Handlers;

public class UpdateDisputeStatusHandler : IUpdateHandler<Dispute, UpdateDisputeStatusForModeratorVM, DisputeVM>
{
    public Task<Result<DisputeVM?>> HandleAsync(Dispute existingEntity, UpdateDisputeStatusForModeratorVM updateModel,
        CancellationToken cancellationToken)
    {
        existingEntity.Status = (DisputeStatus)updateModel.Status;

        return Task.FromResult(Result<DisputeVM?>.Ok());
    }
}