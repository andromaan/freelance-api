using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Quotes;
using DAL.Data;
using Domain.Models.Projects;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class QuoteRepository(AppDbContext context, IUserProvider provider)
    : Repository<Quote, Guid>(context, provider), IQuoteRepository, IQuoteQueries
{
    private readonly AppDbContext _context = context;
    
    public async Task<IEnumerable<Quote>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByFreelancerIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .AsNoTracking()
            .Where(x => x.CreatedBy == userId)
            .OrderByDescending(x => x.ModifiedAt)
            .ToListAsync(cancellationToken);
    }
}