using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.ContractMilestone;
using Domain.Models.Contracts;

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
    IContractRepository contractRepository
)
    : IUpdateHandler<ContractMilestone, UpdContractMilestoneStatusFreelancerVM, ContractMilestoneVM>
{
    public async Task<ServiceResponse<ContractMilestoneVM?>> HandleAsync(
        ContractMilestone existingEntity,
        UpdContractMilestoneStatusFreelancerVM updateModel,
        CancellationToken cancellationToken)
    {
        // Перевірка прав доступу
        var userId = await userProvider.GetUserId();
        var contract = await contractQueries.GetByIdAsync(existingEntity.ContractId, cancellationToken);
        var freelancer = await freelancerQueries.GetByUserIdAsync(userId, cancellationToken);

        if (contract!.FreelancerId != freelancer!.Id)
        {
            return ServiceResponse<ContractMilestoneVM?>.Forbidden("You do not have permission to edit this entity");
        }

        // Processing: Update contract status if first milestone is in progress
        var contractStatusChangeResult =
            await UpdateContractStatusIfNeeded(existingEntity, contract, updateModel, cancellationToken);
        if (contractStatusChangeResult != null)
            return contractStatusChangeResult;


        existingEntity.Status = (ContractMilestoneStatus)updateModel.Status;

        return ServiceResponse<ContractMilestoneVM?>.Ok(); // Валідація пройшла успішно
    }

    private async Task<ServiceResponse<ContractMilestoneVM?>?> UpdateContractStatusIfNeeded(ContractMilestone existingEntity,
        Contract contract, UpdContractMilestoneStatusFreelancerVM updateModel, CancellationToken cancellationToken)
    {
        var contractMilestonesByContract =
            (await contractMilestoneQueries.GetByContractIdAsync(existingEntity.ContractId, cancellationToken))
            .ToList();

        if (contractMilestonesByContract.All(m => m.Status == ContractMilestoneStatus.Pending)
            && updateModel.Status == ContractMilestoneFreelancerStatus.InProgress)
        {
            contract.Status = ContractStatus.InProgress;

            try
            {
                await contractRepository.UpdateAsync(contract, cancellationToken);
            }
            catch (Exception e)
            {
                return ServiceResponse<ContractMilestoneVM?>.InternalError(e.Message);
            }
        }
        
        return null;
    }
}