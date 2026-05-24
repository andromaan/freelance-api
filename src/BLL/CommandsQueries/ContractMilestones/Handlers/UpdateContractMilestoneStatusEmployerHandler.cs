using System.Net;
using AutoMapper;
using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.ContractPayments;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.UserWallets;
using BLL.Common.Interfaces.Repositories.WalletTransactions;
using BLL.Services;
using BLL.ViewModels.ContractMilestone;
using Domain.Models.Contracts;
using Domain.Models.Payments;
using Domain.Models.Projects;

namespace BLL.CommandsQueries.ContractMilestones.Handlers;

public class UpdateContractMilestoneStatusEmployerHandler(
    IUserWalletRepository userWalletRepository,
    IWalletTransactionRepository walletTransactionRepository,
    IContractQueries contractQueries,
    IContractRepository contractRepository,
    IFreelancerQueries freelancerQueries,
    IContractMilestoneQueries contractMilestoneQueries,
    IMapper mapper,
    IContractPaymentRepository contractPaymentRepository,
    IProjectRepository projectRepository,
    IProjectQueries projectQueries
) : IUpdateHandler<ContractMilestone, UpdContractMilestoneStatusEmployerVM, ContractMilestoneVM>
{
    public async Task<Result<ContractMilestoneVM?>> HandleAsync(
        ContractMilestone existingEntity,
        UpdContractMilestoneStatusEmployerVM updateModel,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateMilestoneStatus(existingEntity);
        if (validationError is not null)
            return validationError;

        var contract = await contractQueries.GetByIdAsync(existingEntity.ContractId, cancellationToken);

        var otherMilestones = (await contractMilestoneQueries
                .GetByContractIdAsync(existingEntity.ContractId, cancellationToken))
            .Where(m => m.Id != existingEntity.Id)
            .ToList();

        var areAllOtherMilestonesFinished = otherMilestones
            .All(m => m.Status is ContractMilestoneStatus.Approved or ContractMilestoneStatus.Rejected);

        if (updateModel.Status == ContractMilestoneEmployerStatus.Approved)
        {
            var paymentError = await ProcessPaymentAsync(
                existingEntity, contract!, otherMilestones, areAllOtherMilestonesFinished, cancellationToken);

            if (paymentError is not null)
                return paymentError;
        }

        if (areAllOtherMilestonesFinished && updateModel.Status == ContractMilestoneEmployerStatus.Approved)
        {
            var completionError = await CompleteContractAndProjectAsync(contract!, cancellationToken);
            if (completionError is not null)
                return completionError;
        }

        mapper.Map(updateModel, existingEntity);
        
        return Result<ContractMilestoneVM?>.Ok();
    }

    private static Result<ContractMilestoneVM?>? ValidateMilestoneStatus(
        ContractMilestone milestone)
    {
        if (milestone.Status == ContractMilestoneStatus.Approved)
            return BadRequest("Cannot change the status of an approved contract milestone.");

        if (milestone.Status != ContractMilestoneStatus.Submitted)
            return BadRequest("Only milestones with 'Submitted' status can be updated by the employer.");

        return null;
    }

    // --- Contract & Project Completion ---

    private async Task<Result<ContractMilestoneVM?>?> CompleteContractAndProjectAsync(
        Contract contract,
        CancellationToken cancellationToken)
    {
        contract.Status = ContractStatus.Completed;

        var project = await projectQueries.GetByIdAsync(contract.ProjectId, cancellationToken);
        project!.Status = ProjectStatus.Completed;

        try
        {
            await projectRepository.UpdateAsync(project, cancellationToken);
            await contractRepository.UpdateAsync(contract, cancellationToken);
        }
        catch (Exception e)
        {
            return Result<ContractMilestoneVM?>.InternalError(e.Message);
        }

        return null;
    }

    // --- Payment Processing ---

    private async Task<Result<ContractMilestoneVM?>?> ProcessPaymentAsync(
        ContractMilestone milestone,
        Contract contract,
        List<ContractMilestone> otherMilestones,
        bool isLastMilestone,
        CancellationToken cancellationToken)
    {
        var freelancer = await freelancerQueries.GetByIdAsync(contract.FreelancerId, cancellationToken);
        var freelancerUserId = freelancer!.CreatedBy;

        var milestoneError = await ProcessMilestonePaymentAsync(
            milestone, contract, freelancerUserId, cancellationToken);

        if (milestoneError is not null)
            return milestoneError;

        if (isLastMilestone)
        {
            var remainingAmount = contract.AgreedRate - otherMilestones.Sum(m => m.Amount) - milestone.Amount;

            if (remainingAmount > 0)
            {
                var finalError = await ProcessFinalPaymentAsync(
                    milestone, contract, freelancerUserId, remainingAmount, cancellationToken);

                if (finalError is not null)
                    return finalError;
            }
        }

        return null;
    }

    private async Task<Result<ContractMilestoneVM?>?> ProcessMilestonePaymentAsync(
        ContractMilestone milestone,
        Contract contract,
        Guid freelancerUserId,
        CancellationToken cancellationToken)
    {
        var employerWallet = await userWalletRepository.WithdrawAsync(
            milestone.CreatedBy, milestone.Amount, cancellationToken);

        if (employerWallet is null)
            return BadRequest("Insufficient funds in the employer's wallet to approve this milestone.");

        var freelancerWallet = await userWalletRepository.DepositAsync(
            freelancerUserId, milestone.Amount, cancellationToken);

        await RecordTransferAsync(
            employerWallet.Id, freelancerWallet!.Id,
            milestone.Amount,
            debitType: "Payment for milestone",
            creditType: "Received payment for milestone",
            cancellationToken);

        await RecordContractPaymentAsync(contract.Id, milestone.Id, milestone.Amount, cancellationToken);

        return null;
    }

    private async Task<Result<ContractMilestoneVM?>?> ProcessFinalPaymentAsync(
        ContractMilestone milestone,
        Contract contract,
        Guid freelancerUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var employerWallet = await userWalletRepository.WithdrawAsync(
            milestone.CreatedBy, amount, cancellationToken);

        if (employerWallet is null)
            return BadRequest("Insufficient funds in the employer's wallet to process the final payment for contract completion.");

        var freelancerWallet = await userWalletRepository.DepositAsync(
            freelancerUserId, amount, cancellationToken);

        await RecordTransferAsync(
            employerWallet.Id, freelancerWallet!.Id,
            amount,
            debitType: "Final payment for contract completion",
            creditType: "Received final payment for contract completion",
            cancellationToken);

        await RecordContractPaymentAsync(contract.Id, milestone.Id, amount, cancellationToken);

        return null;
    }

    // --- Helpers ---

    private async Task RecordTransferAsync(
        Guid fromWalletId,
        Guid toWalletId,
        decimal amount,
        string debitType,
        string creditType,
        CancellationToken cancellationToken)
    {
        await walletTransactionRepository.CreateAsync(
            CreateWalletTransaction(-amount, debitType, fromWalletId), cancellationToken);

        await walletTransactionRepository.CreateAsync(
            CreateWalletTransaction(amount, creditType, toWalletId), cancellationToken);
    }

    private async Task RecordContractPaymentAsync(
        Guid contractId,
        Guid milestoneId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        await contractPaymentRepository.CreateAsync(new ContractPayment
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            MilestoneId = milestoneId,
            Amount = amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Wallet"
        }, cancellationToken);
    }

    private static WalletTransaction CreateWalletTransaction(decimal amount, string type, Guid walletId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            TransactionDate = DateTime.UtcNow,
            TransactionType = type,
            WalletId = walletId
        };

    private static Result<ContractMilestoneVM?> BadRequest(string message) =>
        Result<ContractMilestoneVM?>.GetResponse(message, false, null, HttpStatusCode.BadRequest);
}