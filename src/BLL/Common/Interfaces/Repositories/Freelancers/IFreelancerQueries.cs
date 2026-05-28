using Domain.Models.Freelance;

namespace BLL.Common.Interfaces.Repositories.Freelancers;

public interface IFreelancerQueries : IQueries<Freelancer, Guid>, IByUserQuery<Freelancer, Guid>
{
    Task<Freelancer?> GetByUserIdAsync(Guid userId, CancellationToken token, bool includes = false);
    Task<(int TotalCount, List<(Freelancer Freelancer, Domain.Models.Users.User User, decimal Rating, int ReviewsCount)> Items)> SearchFreelancersAsync(
        BLL.ViewModels.Freelancer.FilterFreelancerVM filter, 
        int page, 
        int pageSize, 
        CancellationToken token);
}