using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.Services.Notifications;
using BLL.ViewModels.ContractMilestone;
using Domain.Models.Contracts;
using Domain.Models.Notifications;

namespace BLL.CommandsQueries.ContractMilestones.Handlers;

/// <summary>
/// Unified handler for ContractMilestone status update that combines validation and processing.
/// Replaces UpdateContractMilestoneStatusValidator.
/// </summary>
public class UpdateContractMilestoneStatusFreelancerHandler(
    IUserProvider userProvider,
    IContractQueries contractQueries,
    IFreelancerQueries freelancerQueries,
    IContractMilestoneQueries contractMilestoneQueries,
    IContractRepository contractRepository,
    INotificationService notificationService
)
    : IUpdateHandler<ContractMilestone, UpdContractMilestoneStatusFreelancerVM, ContractMilestoneVM>
{
    public async Task<Result<ContractMilestoneVM?>> HandleAsync(
        ContractMilestone existingEntity,
        UpdContractMilestoneStatusFreelancerVM updateModel,
        CancellationToken cancellationToken)
    {
        // Перевірка прав доступу
        var userId = await userProvider.GetUserId(cancellationToken);
        var contract = await contractQueries.GetByIdAsync(existingEntity.ContractId, cancellationToken);
        var freelancer = await freelancerQueries.GetByUserIdAsync(userId, cancellationToken);

        if (contract!.FreelancerId != freelancer!.Id)
        {
            return Result<ContractMilestoneVM?>.Forbidden("You do not have permission to edit this entity");
        }

        // Processing: Update contract status if first milestone is in progress
        var contractStatusChangeResult =
            await UpdateContractStatusIfNeeded(existingEntity, contract, updateModel, cancellationToken);
        if (contractStatusChangeResult != null)
            return contractStatusChangeResult;


        existingEntity.Status = (ContractMilestoneStatus)updateModel.Status;

        await notificationService.SendAsync(
            message: $"Status changed to '{updateModel.Status}' for contract milestone '{existingEntity.Description}'",
            type: NotificationType.MilestoneStatusUpdated,
            userId: contract.CreatedBy,
            cancellationToken: cancellationToken,
            linkAddress: $"/contract/{contract.Id}");

        return Result<ContractMilestoneVM?>.Ok(); // Валідація пройшла успішно
    }

    private async Task<Result<ContractMilestoneVM?>?> UpdateContractStatusIfNeeded(ContractMilestone existingEntity,
        Contract contract, UpdContractMilestoneStatusFreelancerVM updateModel, CancellationToken cancellationToken)
    {
        var contractMilestonesByContract =
            (await contractMilestoneQueries.GetByContractIdAsync(existingEntity.ContractId, cancellationToken))
            .ToList();

        if (contractMilestonesByContract.All(m => m.Status == ContractMilestoneStatus.Pending)
            && updateModel.Status == ContractMilestoneFreelancerStatus.InProgress)
        {
            contract.Status = ContractStatus.Active;

            try
            {
                await contractRepository.UpdateAsync(contract, cancellationToken);
            }
            catch (Exception e)
            {
                return Result<ContractMilestoneVM?>.InternalError(e.Message);
            }
        }

        return null;
    }
}