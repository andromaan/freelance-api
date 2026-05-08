using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.RefreshTokens;
using DAL.Data;
using Domain.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RefreshTokenRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<RefreshToken, string>(appDbContext, userProvider), IRefreshTokenRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken token)
    {
        var entity = await _appDbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, token);

        return entity;
    }

    public async Task MakeAllRefreshTokensExpiredForUser(Guid userId, CancellationToken token)
    {
        await _appDbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ExecuteUpdateAsync(setters 
                => setters.SetProperty(t => t.IsUsed, true), token);
    }
}