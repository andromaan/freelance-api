using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetContractByUserQuery : IRequest<Result<List<ContractVM>>>;

public class QueryHandler(IContractQueries contractQueries, IMapper mapper)
    : IRequestHandler<GetContractByUserQuery, Result<List<ContractVM>>>
{
    public async Task<Result<List<ContractVM>>> Handle(GetContractByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var contracts = await contractQueries.GetByUser(cancellationToken);

            return Result<List<ContractVM>>.Ok("Contracts retrieved", mapper.Map<List<ContractVM>>(contracts));
        }
        catch (Exception exception)
        {
            return Result<List<ContractVM>>.InternalError(exception.Message);
        }
    }
}