using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Bids;
using DAL.Data;
using Domain.Models.Projects;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BidRepository(AppDbContext context, IUserProvider provider)
    : Repository<Bid, Guid>(context, provider), IBidRepository, IBidQueries
{
    private readonly AppDbContext _context = context;
    
    public async Task<IEnumerable<Bid>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Bid>()
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.ModifiedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bid>> GetByFreelancerIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Bid>()
            .AsNoTracking()
            .Where(x => x.CreatedBy == userId)
            .OrderByDescending(x => x.ModifiedAt)
            .ToListAsync(cancellationToken);
    }
}