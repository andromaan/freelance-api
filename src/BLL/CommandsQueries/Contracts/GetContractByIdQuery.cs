using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Roles;
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
    IFreelancerQueries freelancerQueries,
    IRoleQueries roleQueries)
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
            var user = await userProvider.GetUser(cancellationToken);
            var freelancer = await freelancerQueries.GetByUserIdAsync(user!.Id, cancellationToken);

            var isNotFreelancer = contract.FreelancerId != freelancer?.Id;
            var isNotEmployer = contract.CreatedBy != user.Id;


            if (isNotFreelancer && isNotEmployer && user.Role!.Name != Settings.Roles.AdminRole &&
                user.Role.Name != Settings.Roles.ModeratorRole)
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