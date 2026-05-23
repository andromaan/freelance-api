using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Wallets;
using BLL.Services;
using BLL.ViewModels.Wallet;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WalletController(ISender sender) : BaseController
{
    [HttpGet("balance")]
    public async Task<ActionResult<ServiceResponse<UserWalletVM>>> GetBalance(CancellationToken ct)
    {
        var result = await sender.Send(new GetWalletBalanceQuery(), ct);
        return GetResult(result);
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for depositing money into the user's wallet.
    /// Returns <c>clientSecret</c> (use with Stripe.js on the frontend to confirm the payment)
    /// and <c>paymentIntentId</c> (pass to /confirm-deposit after successful payment).
    /// </summary>
    [Authorize(Policy = Settings.Roles.AdminOrEmployer)]
    [HttpPost("create-payment-intent")]
    public async Task<ActionResult<ServiceResponse<object>>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentVM vm,
        CancellationToken ct)
    {
        var command = new CreatePaymentIntentCommand(vm);
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    /// <summary>
    /// Confirms a completed Stripe payment and credits the amount to the user's wallet.
    /// Must be called after the frontend has successfully confirmed the payment via Stripe.js.
    /// </summary>
    [Authorize(Policy = Settings.Roles.AdminOrEmployer)]
    [HttpPost("confirm-deposit")]
    public async Task<ActionResult<ServiceResponse<object>>> ConfirmDeposit(
        [FromBody] ConfirmDepositVM vm,
        CancellationToken ct)
    {
        var command = new ConfirmDepositCommand(vm);
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }
}
