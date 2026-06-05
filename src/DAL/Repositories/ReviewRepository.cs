using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Reviews;
using DAL.Data;
using Domain.Models.Reviews;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ReviewRepository(AppDbContext context, IUserProvider userProvider)
    : Repository<Review, Guid>(context, userProvider), IReviewRepository, IReviewQueries
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<Review>> GetReviewsByReviewedUser(Guid reviewedUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Review>().Where(r => r.ReviewedUserId == reviewedUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetReviewsByReviewerUser(Guid reviewerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Review>().Where(r => r.CreatedBy == reviewerId).ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByReviewerAndReviewedUser(Guid reviewerId, Guid reviewedUserId, Guid contractId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<Review>()
            .FirstOrDefaultAsync(
                r => r.CreatedBy == reviewerId && r.ReviewedUserId == reviewedUserId && r.ContractId == contractId,
                cancellationToken);
    }

    public async Task<Review?> GetReviewByContractAndReviewer(Guid reviewerId, Guid contractId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.CreatedBy == reviewerId && r.ContractId == contractId,
            cancellationToken);
    }
}