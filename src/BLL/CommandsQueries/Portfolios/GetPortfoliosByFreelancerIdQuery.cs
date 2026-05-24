using AutoMapper;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Portfolios;
using BLL.Services;
using BLL.ViewModels.Portfolio;
using MediatR;

namespace BLL.CommandsQueries.Portfolios;

public record GetPortfoliosByFreelancerIdQuery(Guid FreelancerId) : IRequest<Result<List<PortfolioVM>?>>;

public class GetPortfoliosByFreelancerIdQueryQueryHandler(
    IFreelancerQueries queriesFreelancer,
    IMapper mapper,
    IPortfolioQueries queriesPortfolio)
    : IRequestHandler<GetPortfoliosByFreelancerIdQuery, Result<List<PortfolioVM>?>>
{
    public async Task<Result<List<PortfolioVM>?>> Handle(GetPortfoliosByFreelancerIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var freelancer = await queriesFreelancer.GetByIdAsync(request.FreelancerId, cancellationToken);
            if (freelancer == null)
            {
                return Result<List<PortfolioVM>?>.NotFound("Freelancer not found");
            }

            var portfolios = await queriesPortfolio.GetByFreelancerIdAsync(request.FreelancerId, cancellationToken);

            return Result<List<PortfolioVM>?>.Ok("Portfolio's retrieved",
                mapper.Map<List<PortfolioVM>>(portfolios));
        }
        catch (Exception exception)
        {
            return Result<List<PortfolioVM>?>.InternalError(exception.Message);
        }
    }
}