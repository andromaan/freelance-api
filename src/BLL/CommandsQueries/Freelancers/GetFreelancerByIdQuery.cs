using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByIdQuery(Guid FreelancerId) : IRequest<ServiceResponse<FreelancerVM?>>;

public class GetFreelancerByIdQueryQueryHandler(
    IFreelancerQueries queriesFreelancer,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetFreelancerByIdQuery, ServiceResponse<FreelancerVM?>>
{
    public async Task<ServiceResponse<FreelancerVM?>> Handle(GetFreelancerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await userProvider.GetUserId();

            var freelancer = await queriesFreelancer.GetByIdAsync(userId, cancellationToken);
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