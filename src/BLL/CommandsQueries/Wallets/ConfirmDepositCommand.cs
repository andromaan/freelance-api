using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.UserWallets;
using BLL.Common.Interfaces.Repositories.WalletTransactions;
using BLL.Services;
using BLL.Services.StripeService;
using BLL.ViewModels.Wallet;
using Domain.Models.Payments;
using MediatR;

namespace BLL.CommandsQueries.Wallets;

/// <summary>
/// Verifies that the Stripe PaymentIntent has succeeded, then credits the user's wallet.
/// Should be called by the frontend after Stripe.js confirms the payment.
/// </summary>
public record ConfirmDepositCommand(ConfirmDepositVM Vm) : IRequest<Result<object?>>;

public class ConfirmDepositCommandHandler(
    IStripeService stripeService,
    IUserProvider userProvider,
    IUserWalletRepository userWalletRepository,
    IWalletTransactionRepository walletTransactionRepository)
    : IRequestHandler<ConfirmDepositCommand, Result<object?>>
{
    public async Task<Result<object?>> Handle(
        ConfirmDepositCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId(cancellationToken);

            // Retrieve PaymentIntent from Stripe and verify it succeeded
            var paymentIntent = await stripeService.GetPaymentIntentAsync(
                request.Vm.PaymentIntentId,
                cancellationToken);

            if (paymentIntent.Status != "succeeded")
                return Result<object?>.BadRequest(
                    $"Payment has not been completed. Current status: '{paymentIntent.Status}'. " +
                    "Please confirm payment on the client side first.");

            // Amount comes from Stripe (in cents) — convert back to decimal
            var amount = paymentIntent.Amount / 100m;
            var currency = paymentIntent.Currency.ToUpper();

            // Credit the wallet
            var wallet = await userWalletRepository.DepositAsync(userId, amount, cancellationToken);
            if (wallet is null)
                return Result<object?>.NotFound("Wallet not found for current user.");
            
            // Record the transaction
            await walletTransactionRepository.CreateAsync(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                TransactionType = "Deposit",
                TransactionDate = DateTime.UtcNow
            }, cancellationToken);

            return Result<object?>.Ok("Deposit successful.", new
            {
                NewBalance = wallet.Balance,
                DepositedAmount = amount,
                Currency = currency
            });
        }
        catch (Exception ex)
        {
            return Result<object?>.InternalError(ex.Message);
        }
    }
}
