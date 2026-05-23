using Domain.Models.Projects;

namespace BLL.Common.Interfaces.Repositories.Quotes;

public interface IQuoteQueries : IQueries<Quote, Guid>
{
    Task<IEnumerable<Quote>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByFreelancerIdAsync(Guid userId, CancellationToken cancellationToken = default);
}