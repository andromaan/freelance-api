using Domain.Models.Contracts;

namespace BLL.Common.Interfaces.Repositories.Contracts;

public interface IContractQueries : IQueries<Contract, Guid>
{
    Task<IEnumerable<Contract>> GetByUser(CancellationToken cancellationToken);
    Task<IEnumerable<Contract>> GetByFreelancerId(Guid freelancerId, CancellationToken cancellationToken);

    Task<bool> IsContractCanBeCreated(Guid projectId, Guid createdById, Guid freelancerId,
        CancellationToken cancellationToken);

    Task<bool> IsExistsByQuoteQuery(Guid projectId, Guid createdById, Guid freelancerId,
        CancellationToken cancellationToken);
}