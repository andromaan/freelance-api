using AutoMapper;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByEmailQuery(string Email) : IRequest<ServiceResponse<FreelancerVM?>>;

public class GetFreelancerByEmailQueryHandler(
    IFreelancerQueries queriesFreelancer,
    IMapper mapper,
    IUserQueries userQueries)
    : IRequestHandler<GetFreelancerByEmailQuery, ServiceResponse<FreelancerVM?>>
{
    public async Task<ServiceResponse<FreelancerVM?>> Handle(GetFreelancerByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userQueries.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                return ServiceResponse<FreelancerVM?>.NotFound("User not found with the provided email");
            }

            var freelancer = await queriesFreelancer.GetByUserIdAsync(user.Id, cancellationToken);
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