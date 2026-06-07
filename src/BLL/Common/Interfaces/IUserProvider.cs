using Domain.Models.Users;

namespace BLL.Common.Interfaces;

public interface IUserProvider
{
    Task<Guid> GetUserId(CancellationToken cancellationToken = default);
    Task<User?> GetUser(CancellationToken cancellationToken = default);
    string GetUserRole();
}