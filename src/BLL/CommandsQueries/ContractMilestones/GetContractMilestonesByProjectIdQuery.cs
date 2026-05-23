using AutoMapper;
using BLL.Common.Interfaces.Repositories.ContractMilestones;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Services;
using BLL.ViewModels.ContractMilestone;
using MediatR;

namespace BLL.CommandsQueries.ContractMilestones;

public record GetContractMilestonesByContractIdQuery : IRequest<ServiceResponse<List<ContractMilestoneVM>?>>
{
    public required Guid ContractId { get; init; }
}

public class QueryHandler(
    IContractMilestoneQueries contractMilestoneService,
    IContractQueries contractQueries,
    IMapper mapper)
    : IRequestHandler<GetContractMilestonesByContractIdQuery, ServiceResponse<List<ContractMilestoneVM>?>>
{
    public async Task<ServiceResponse<List<ContractMilestoneVM>?>> Handle(GetContractMilestonesByContractIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingContract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken, true);
        if (existingContract == null)
        {
            return ServiceResponse<List<ContractMilestoneVM>?>.NotFound($"Contract with id {request.ContractId} not found");
        }

        var result = await contractMilestoneService.GetByContractIdAsync(request.ContractId, cancellationToken);
        return ServiceResponse<List<ContractMilestoneVM>?>.Ok("Contract milestones receive successfully",
            mapper.Map<List<ContractMilestoneVM>>(result));
    }
}