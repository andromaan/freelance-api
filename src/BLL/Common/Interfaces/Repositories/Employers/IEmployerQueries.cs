using Domain.Models.Employers;

namespace BLL.Common.Interfaces.Repositories.Employers;

public interface IEmployerQueries : IQueries<Employer, Guid>, IByUserQuery<Employer, Guid>
{
    Task<Employer?> GetByUserIdAsync(Guid userId, CancellationToken token);
    Task<Employer?> GetByUserId(Guid userId, CancellationToken token);
}