using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Portfolio;
using Domain.Models.Freelance;

namespace BLL.CommandsQueries.Portfolios.Handlers;

public class CreatePortfolioHandler(
    IUserProvider userProvider,
    IFreelancerQueries freelancerQueries)
    : ICreateHandler<Portfolio, CreatePortfolioVM, PortfolioVM>
{
    public async Task<Result<PortfolioVM?>> HandleAsync(Portfolio entity, CreatePortfolioVM createModel,
        CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId();
        
        var freelancer = await freelancerQueries.GetByUserIdAsync(userId, cancellationToken);
        
        entity.FreelancerId = freelancer!.Id;
        
        return Result<PortfolioVM?>.Ok();
    }
}