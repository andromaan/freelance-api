using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetContractByUserQuery : IRequest<ServiceResponse<List<ContractVM>>>;

public class QueryHandler(IContractQueries contractQueries, IMapper mapper)
    : IRequestHandler<GetContractByUserQuery, ServiceResponse<List<ContractVM>>>
{
    public async Task<ServiceResponse<List<ContractVM>>> Handle(GetContractByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var contracts = await contractQueries.GetByUser(cancellationToken);

            return ServiceResponse<List<ContractVM>>.Ok("Contracts retrieved", mapper.Map<List<ContractVM>>(contracts));
        }
        catch (Exception exception)
        {
            return ServiceResponse<List<ContractVM>>.InternalError(exception.Message);
        }
    }
}