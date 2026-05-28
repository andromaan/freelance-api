using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetCompletedContractByFreelancerIdQuery(Guid FreelancerId) : IRequest<Result<List<ContractVM>>>;

public class GetContractByFreelancerIdQueryQueryHandler(IContractQueries contractQueries, IMapper mapper)
    : IRequestHandler<GetCompletedContractByFreelancerIdQuery, Result<List<ContractVM>>>
{
    public async Task<Result<List<ContractVM>>> Handle(GetCompletedContractByFreelancerIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var contracts =
                await contractQueries.GetByFreelancerId(request.FreelancerId, cancellationToken);

            var filteredContracts = contracts.Where(
                    c => c.Status == Domain.Models.Contracts.ContractStatus.Completed).ToList();

            return Result<List<ContractVM>>.Ok("Contracts by freelancer retrieved",
                mapper.Map<List<ContractVM>>(filteredContracts));
        }
        catch (Exception exception)
        {
            return Result<List<ContractVM>>.InternalError(exception.Message);
        }
    }
}