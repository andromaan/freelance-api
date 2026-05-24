using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetContractByFreelancerIdQuery(Guid FreelancerId) : IRequest<Result<List<ContractVM>>>;

public class GetContractByFreelancerIdQueryQueryHandler(IContractQueries contractQueries, IMapper mapper)
    : IRequestHandler<GetContractByFreelancerIdQuery, Result<List<ContractVM>>>
{
    public async Task<Result<List<ContractVM>>> Handle(GetContractByFreelancerIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var contracts = 
                await contractQueries.GetByFreelancerId(request.FreelancerId, cancellationToken);

            return Result<List<ContractVM>>.Ok("Contracts by freelancer retrieved", 
                mapper.Map<List<ContractVM>>(contracts));
        }
        catch (Exception exception)
        {
            return Result<List<ContractVM>>.InternalError(exception.Message);
        }
    }
}