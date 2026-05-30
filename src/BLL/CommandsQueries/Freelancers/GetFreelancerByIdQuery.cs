using AutoMapper;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;

namespace BLL.CommandsQueries.Freelancers;

public record GetFreelancerByIdQuery(Guid FreelancerId) : IRequest<Result<FreelancerVM?>>;

public class GetFreelancerByIdQueryQueryHandler(
    IFreelancerQueries queriesFreelancer,
    IMapper mapper)
    : IRequestHandler<GetFreelancerByIdQuery, Result<FreelancerVM?>>
{
    public async Task<Result<FreelancerVM?>> Handle(GetFreelancerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var freelancer = await queriesFreelancer.GetByIdAsync(request.FreelancerId, cancellationToken);
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