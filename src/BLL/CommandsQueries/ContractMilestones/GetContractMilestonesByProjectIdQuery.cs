using AutoMapper;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.ContractMilestone;
using MediatR;

namespace BLL.CommandsQueries.ContractMilestones;

public record GetContractMilestonesByContractIdQuery : IRequest<Result<List<ContractMilestoneVM>?>>
{
    public required Guid ContractId { get; init; }
}

public class QueryHandler(
    IContractMilestoneQueries contractMilestoneService,
    IContractQueries contractQueries,
    IMapper mapper)
    : IRequestHandler<GetContractMilestonesByContractIdQuery, Result<List<ContractMilestoneVM>?>>
{
    public async Task<Result<List<ContractMilestoneVM>?>> Handle(GetContractMilestonesByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingContract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken, true);
        if (existingContract == null)
        {
            return Result<List<ContractMilestoneVM>?>.NotFound($"Contract with id {request.ContractId} not found");
        }

        var result = await contractMilestoneService.GetByContractIdAsync(request.ContractId, cancellationToken);
        return Result<List<ContractMilestoneVM>?>.Ok("Contract milestones receive successfully",
            mapper.Map<List<ContractMilestoneVM>>(result));
    }
}