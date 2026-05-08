using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using DAL.Data;
using Domain.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ContractRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<Contract, Guid>(appDbContext, userProvider), IContractRepository, IContractQueries
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IUserProvider _userProvider = userProvider;


    public async Task<IEnumerable<Contract>> GetByUser(CancellationToken cancellationToken)
    {
        var userId = _userProvider.GetUserId().GetAwaiter().GetResult();

        return await _appDbContext.Contracts
            .Include(c => c.Freelancer)
            .Where(p => p.CreatedBy == userId || p.Freelancer!.CreatedBy == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetByFreelancerId(Guid freelancerId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Contracts
            .Include(c => c.Project).ThenInclude(p => p!.Categories)
            .Where(p => p.FreelancerId == freelancerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}