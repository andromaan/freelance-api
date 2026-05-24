using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.UserWallets;
using BLL.Services;
using BLL.ViewModels.Wallet;
using MediatR;

namespace BLL.CommandsQueries.Wallets;

/// <summary>
/// Returns the current balance of the authenticated user's wallet.
/// </summary>
public record GetWalletBalanceQuery : IRequest<Result<UserWalletVM?>>;

public class GetWalletBalanceQueryHandler(
    IUserProvider userProvider,
    IUserWalletQueries userWalletQueries,
    IMapper mapper)
    : IRequestHandler<GetWalletBalanceQuery, Result<UserWalletVM?>>
{
    public async Task<Result<UserWalletVM?>> Handle(
        GetWalletBalanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId(cancellationToken);
            var userWallet = await userWalletQueries.GetByUserIdAsync(userId, cancellationToken);

            if (userWallet is null)
                return Result<UserWalletVM?>.NotFound("Wallet not found for current user.");

            return Result<UserWalletVM?>.Ok("Wallet balance retrieved.", mapper.Map<UserWalletVM>(userWallet));
        }
        catch (Exception ex)
        {
            return Result<UserWalletVM?>.InternalError(ex.Message);
        }
    }
}