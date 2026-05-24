using AutoMapper;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByEmailQuery(string Email) : IRequest<Result<FreelancerVM?>>;

public class GetFreelancerByEmailQueryHandler(
    IFreelancerQueries queriesFreelancer,
    IMapper mapper,
    IUserQueries userQueries)
    : IRequestHandler<GetFreelancerByEmailQuery, Result<FreelancerVM?>>
{
    public async Task<Result<FreelancerVM?>> Handle(GetFreelancerByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userQueries.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                return Result<FreelancerVM?>.NotFound("User not found with the provided email");
            }

            var freelancer = await queriesFreelancer.GetByUserIdAsync(user.Id, cancellationToken);
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