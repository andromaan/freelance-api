using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetContractByFreelancerIdQuery(Guid FreelancerId) : IRequest<ServiceResponse<List<ContractVM>>>;

public class GetContractByFreelancerIdQueryQueryHandler(IContractQueries contractQueries, IMapper mapper)
    : IRequestHandler<GetContractByFreelancerIdQuery, ServiceResponse<List<ContractVM>>>
{
    public async Task<ServiceResponse<List<ContractVM>>> Handle(GetContractByFreelancerIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var contracts = 
                await contractQueries.GetByFreelancerId(request.FreelancerId, cancellationToken);

            return ServiceResponse<List<ContractVM>>.Ok("Contracts by freelancer retrieved", 
                mapper.Map<List<ContractVM>>(contracts));
        }
        catch (Exception exception)
        {
            return ServiceResponse<List<ContractVM>>.InternalError(exception.Message);
        }
    }
}