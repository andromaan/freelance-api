using System.Linq.Expressions;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.ViewModels.User;
using DAL.Data;
using Domain.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<User, Guid>(appDbContext, userProvider), IUserRepository, IUserQueries
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken token, bool includes = true)
    {
        return await GetUserAsync(u => u.Email == email, token, includes);
    }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken token, bool asNoTracking = false)
    {
        return await GetUserAsync(u => u.Id == id, token);
    }

    private async Task<User?> GetUserAsync(Expression<Func<User, bool>> predicate, CancellationToken token,
        bool includes = true)
    {
        if (includes)
        {
            return await _appDbContext.Users
                .Include(ur => ur.Role)
                .Include(u => u.Languages)
                .Include(u => u.Country)
                .FirstOrDefaultAsync(predicate, token);
        }

        return await _appDbContext.Users
            .FirstOrDefaultAsync(predicate, token);
    }

    public async Task<List<User>> GetUsersByRoleAsync(string roleName, CancellationToken token = default)
    {
        return await _appDbContext.Users
            .Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Name == roleName)
            .ToListAsync(token);
    }

    public async Task<User?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        var user = await _appDbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.ExternalProvider == loginProvider &&
                     u.ExternalProviderKey == providerKey,
                cancellationToken);

        return user;
    }

    public async Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo loginInfo,
        CancellationToken cancellationToken)
    {
        var userEntity = await GetUserAsync(x => x.Id == user.Id, cancellationToken);

        if (userEntity == null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
        }

        userEntity.ExternalProvider = loginInfo.LoginProvider;
        userEntity.ExternalProviderKey = loginInfo.ProviderKey;

        await _appDbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<bool> IsUniqueEmailAsync(string email, CancellationToken token)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email, token) == null;
    }

    public async Task<User?> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await GetUserAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<(int TotalCount, List<User> Items)> SearchUsersAsync(FilterUserVM filter, int page, int pageSize, CancellationToken token)
    {
        var query = _appDbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Languages)
            .Include(u => u.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchStr = filter.Search.ToLower();
            query = query.Where(u => (u.DisplayName != null && u.DisplayName.ToLower().Contains(searchStr)) || u.Email.ToLower().Contains(searchStr));
        }

        if (filter.RoleIds != null && filter.RoleIds.Any())
        {
            query = query.Where(u => filter.RoleIds.Contains(u.RoleId));
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync(token);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(token);

        return (totalCount, items);
    }
}