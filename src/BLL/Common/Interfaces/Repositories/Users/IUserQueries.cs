using BLL.ViewModels.User;
using Domain.Models.Users;

namespace BLL.Common.Interfaces.Repositories.Users;

public interface IUserQueries : IQueries<User, Guid>, IByUserQuery<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken token, bool includes = true);
    Task<bool> IsUniqueEmailAsync(string email, CancellationToken token);
    Task<List<User>> GetUsersByRoleAsync(string roleName, CancellationToken token = default);
    Task<User?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken);
    Task<(int TotalCount, List<User> Items)> SearchUsersAsync(FilterUserVM filter, int page, int pageSize, CancellationToken token);
}