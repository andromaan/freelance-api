using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Services;
using BLL.ViewModels.Contract;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record GetContractByIdQuery : IRequest<Result<ContractVM?>>
{
    public required Guid ContractId { get; init; }
}

public class GetContractByIdQueryQueryHandler(
    IContractQueries contractQueries,
    IMapper mapper,
    IUserProvider userProvider,
    IFreelancerQueries freelancerQueries)
    : IRequestHandler<GetContractByIdQuery, Result<ContractVM?>>
{
    public async Task<Result<ContractVM?>> Handle(GetContractByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var contract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken);

            if (contract == null)
            {
                return Result<ContractVM?>.NotFound("Contract not found");
            }

            // Перевірка прав доступу
            var userId = await userProvider.GetUserId(cancellationToken);
            var freelancer = await freelancerQueries.GetByUserIdAsync(userId, cancellationToken);

            if (contract.FreelancerId != freelancer?.Id && contract.CreatedBy != userId)
            {
                return Result<ContractVM?>.Forbidden("You do not have permission to edit this entity");
            }

            return Result<ContractVM?>.Ok("Contracts retrieved", mapper.Map<ContractVM?>(contract));
        }
        catch (Exception exception)
        {
            return Result<ContractVM?>.InternalError(exception.Message);
        }
    }
}