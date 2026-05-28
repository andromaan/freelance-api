using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Freelancers;
using DAL.Data;
using DAL.Extensions;
using Domain.Models.Freelance;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class FreelancerRepository(AppDbContext appDbContext, IUserProvider userProvider)
    : Repository<Freelancer, Guid>(appDbContext, userProvider), IFreelancerRepository, IFreelancerQueries
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public override Task<Freelancer?> GetByIdAsync(Guid id, CancellationToken token, bool asNoTracking = false)
    {
        return base.GetByIdAsync(id, token, asNoTracking,
            freelancer => freelancer.Portfolio,
            freelancer => freelancer.Skills);
    }

    public async Task<Freelancer?> CreateAsync(Freelancer entity, Guid createdBy, CancellationToken token)
    {
        entity.CreatedBy = createdBy;
        await _appDbContext.AddAuditableAsync(entity, token);
        await _appDbContext.SaveChangesAsync(token);
        return entity;
    }

    public async Task<Freelancer?> GetByUserIdAsync(Guid userId, CancellationToken token, bool includes = false)
    {
        var query = _appDbContext.Freelancers.AsQueryable();

        if (includes)
        {
            query = query
                .Include(up => up.Skills)
                .Include(up => up.Portfolio);
        }

        return await query.FirstOrDefaultAsync(up => up.CreatedBy == userId, token);
    }

    public async Task<Freelancer?> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Freelancers
            .Include(up => up.Skills)
            .Include(up => up.Portfolio)
            .FirstOrDefaultAsync(up => up.CreatedBy == userId, cancellationToken);
    }

    public async
        Task<(int TotalCount,
            List<(Freelancer Freelancer, Domain.Models.Users.User User, decimal Rating, int ReviewsCount)> Items)>
        SearchFreelancersAsync(
            BLL.ViewModels.Freelancer.FilterFreelancerVM filter,
            int page,
            int pageSize,
            CancellationToken token)
    {
        var query = _appDbContext.Freelancers
            .Include(f => f.Skills)
            .AsNoTracking();

        var usersQuery = _appDbContext.Users
            .Include(u => u.Languages)
            .ThenInclude(ul => ul.Language)
            .Include(u => u.Country)
            .AsNoTracking();

        var joinQuery = from f in query
            join u in usersQuery on f.CreatedBy equals u.Id
            select new { Freelancer = f, User = u };

        if (!string.IsNullOrEmpty(filter.Name))
        {
            joinQuery = joinQuery.Where(x =>
                x.User.DisplayName != null &&
                x.User.DisplayName.Contains(filter.Name));
        }

        if (!string.IsNullOrEmpty(filter.Email))
        {
            joinQuery = joinQuery.Where(x => x.User.Email.Contains(filter.Email));
        }

        if (filter.SkillIds != null && filter.SkillIds.Count > 0)
        {
            joinQuery = joinQuery.Where(x => x.Freelancer.Skills.Any(s => filter.SkillIds.Contains(s.Id)));
        }

        if (filter.LanguageIds != null && filter.LanguageIds.Count > 0)
        {
            joinQuery = joinQuery.Where(x => x.User.Languages.Any(l => filter.LanguageIds.Contains(l.LanguageId)));
        }

        if (filter.CountryIds != null && filter.CountryIds.Count > 0)
        {
            joinQuery = joinQuery.Where(x =>
                x.User.CountryId.HasValue && filter.CountryIds.Contains(x.User.CountryId.Value));
        }

        var joinedData = await joinQuery.ToListAsync(token);

        var userIds = joinedData.Select(x => x.User.Id).ToList();
        var reviews = await _appDbContext.Reviews
            .AsNoTracking()
            .Where(r => userIds.Contains(r.ReviewedUserId))
            .ToListAsync(token);

        var userRatings = reviews
            .GroupBy(r => r.ReviewedUserId)
            .ToDictionary(g => g.Key, g => new { Rating = g.Average(r => r.Rating), Count = g.Count() });

        var resultList =
            new List<(Freelancer Freelancer, Domain.Models.Users.User User, decimal Rating, int ReviewsCount)>();
        foreach (var item in joinedData)
        {
            decimal rating = 0;
            int reviewCount = 0;

            if (userRatings.TryGetValue(item.User.Id, out var r))
            {
                rating = r.Rating;
                reviewCount = r.Count;
            }

            if (filter.MinRating.HasValue && rating < filter.MinRating.Value)
            {
                continue;
            }

            resultList.Add((item.Freelancer, item.User, rating, reviewCount));
        }

        int totalCount = resultList.Count;
        var paged = resultList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, paged);
    }
}