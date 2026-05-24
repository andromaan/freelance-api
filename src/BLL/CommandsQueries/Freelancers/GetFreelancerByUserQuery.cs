using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByUserQuery : IRequest<Result<FreelancerVM?>>;

public class QueryHandler(
    IFreelancerQueries queriesFreelancer,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetFreelancerByUserQuery, Result<FreelancerVM?>>
{
    public async Task<Result<FreelancerVM?>> Handle(GetFreelancerByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();

            var freelancer = await queriesFreelancer.GetByUserIdAsync(userId, cancellationToken, includes: true);
            if (freelancer == null)
            {
                return Result<FreelancerVM?>.NotFound("Freelancer not found");
            }

            return Result<FreelancerVM?>.Ok("Freelancer retrieved",
                mapper.Map<FreelancerVM>(freelancer));
        }
        catch (Exception exception)
        {
            return Result<FreelancerVM?>.InternalError(exception.Message);
        }
    }
}