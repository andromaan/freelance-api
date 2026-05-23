using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByUserQuery : IRequest<ServiceResponse<FreelancerVM?>>;

public class QueryHandler(
    IFreelancerQueries queriesFreelancer,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetFreelancerByUserQuery, ServiceResponse<FreelancerVM?>>
{
    public async Task<ServiceResponse<FreelancerVM?>> Handle(GetFreelancerByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();

            var freelancer = await queriesFreelancer.GetByUserIdAsync(userId, cancellationToken, includes: true);
            if (freelancer == null)
            {
                return ServiceResponse<FreelancerVM?>.NotFound("Freelancer not found");
            }

            return ServiceResponse<FreelancerVM?>.Ok("Freelancer retrieved",
                mapper.Map<FreelancerVM>(freelancer));
        }
        catch (Exception exception)
        {
            return ServiceResponse<FreelancerVM?>.InternalError(exception.Message);
        }
    }
}