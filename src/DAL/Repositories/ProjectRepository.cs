using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Projects;
using DAL.Data;
using Domain.Models.Projects;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ProjectRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<Project, Guid>(appDbContext, userProvider), IProjectRepository, IProjectQueries
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IUserProvider _userProvider = userProvider;

    public override Task<IEnumerable<Project>> GetAllAsync(CancellationToken token)
    {
        return base.GetAllAsync(token, p => p.Categories,
            p => p.Bids, p => p.Quotes);
    }

    public override Task<Project?> GetByIdAsync(Guid id, CancellationToken token, bool asNoTracking = false)
    {
        return base.GetByIdAsync(id, token, asNoTracking,
            p => p.Categories,
            p => p.Bids, p => p.Quotes);
    }

    public async Task<List<Project>> GetByEmployer(CancellationToken cancellationToken)
    {
        var userId = await _userProvider.GetUserId();
        
        return await _appDbContext.Set<Project>().Where(p => p.CreatedBy == userId)
            .Include(p => p.Categories)
            .Include(p => p.Bids)
            .Include(p => p.Quotes)
            .AsNoTracking().ToListAsync(cancellationToken);
    }
}