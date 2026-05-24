using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.ContractMilestone;
using Domain.Models.Contracts;

namespace BLL.CommandsQueries.ContractMilestones.Handlers;

public class UpdateContractMilestoneHandler(
    IContractQueries contractQueries,
    IContractMilestoneQueries milestoneQueries,
    IMapper mapper
    ) : IUpdateHandler<ContractMilestone, UpdateContractMilestoneVM, ContractMilestoneVM>
{
    public async Task<Result<ContractMilestoneVM?>> HandleAsync(
        ContractMilestone existingEntity,
        UpdateContractMilestoneVM updateModel, CancellationToken cancellationToken)
    {
        if (existingEntity is not { Status: ContractMilestoneStatus.Pending })
        {
            return Result<ContractMilestoneVM?>.BadRequest(
                "Only milestones with 'Pending' status can be updated");
        }
        
        // Перевірка чи змінився amount
        if (existingEntity.Amount == updateModel.Amount)
        {
            return Result<ContractMilestoneVM?>.Ok();// Якщо amount не змінився, валідація не потрібна і змінна сутності теж
        }

        // Отримай контракт
        var contract = await contractQueries.GetByIdAsync(
            existingEntity.ContractId, 
            cancellationToken, asNoTracking: true);
            
        if (contract == null)
        {
            return Result<ContractMilestoneVM?>.NotFound(
                $"Contract with ID {existingEntity.ContractId} not found");
        }

        // Отримай всі milestone для контракту
        var allMilestones = await milestoneQueries.GetByContractIdAsync(
            existingEntity.ContractId, 
            cancellationToken);

        // Порахуй загальну суму (виключаючи поточний milestone)
        var totalAmount = allMilestones
            .Where(m => m.Id != existingEntity.Id)
            .Sum(m => m.Amount) + updateModel.Amount;

        // Перевір чи не перевищує бюджет
        if (totalAmount > contract.AgreedRate)
        {
            return Result<ContractMilestoneVM?>.BadRequest(
                $"The total amount ({totalAmount}) of milestones exceeds " +
                $"the contract's agreed rate ({contract.AgreedRate})");
        }
        
        mapper.Map(updateModel, existingEntity);

        return Result<ContractMilestoneVM?>.Ok();  // Валідація пройшла успішно
    }
}