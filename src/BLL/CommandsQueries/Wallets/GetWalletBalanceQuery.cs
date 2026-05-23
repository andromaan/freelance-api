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
public record GetWalletBalanceQuery : IRequest<ServiceResponse<UserWalletVM?>>;

public class GetWalletBalanceQueryHandler(
    IUserProvider userProvider,
    IUserWalletQueries userWalletQueries,
    IMapper mapper)
    : IRequestHandler<GetWalletBalanceQuery, ServiceResponse<UserWalletVM?>>
{
    public async Task<ServiceResponse<UserWalletVM?>> Handle(
        GetWalletBalanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId(cancellationToken);
            var userWallet = await userWalletQueries.GetByUserIdAsync(userId, cancellationToken);

            if (userWallet is null)
                return ServiceResponse<UserWalletVM?>.NotFound("Wallet not found for current user.");

            return ServiceResponse<UserWalletVM?>.Ok("Wallet balance retrieved.", mapper.Map<UserWalletVM>(userWallet));
        }
        catch (Exception ex)
        {
            return ServiceResponse<UserWalletVM?>.InternalError(ex.Message);
        }
    }
}