using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.Project;
using MediatR;

namespace BLL.CommandsQueries.Projects;

public record GetProjectByContractQuery : IRequest<Result<ProjectVM?>>
{
    public required Guid ContractId { get; init; }
}

public class GetProjectByContractQueryHandler(
    IProjectQueries projectQueries,
    IMapper mapper,
    IContractQueries contractQueries)
    : IRequestHandler<GetProjectByContractQuery, Result<ProjectVM?>>
{
    public async Task<Result<ProjectVM?>> Handle(GetProjectByContractQuery request, CancellationToken cancellationToken)
    {
        var contract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return Result<ProjectVM?>.NotFound("Contract not found");
        }

        try
        {
            var projects = await projectQueries.GetByIdAsync(contract.ProjectId, cancellationToken);

            if (projects == null)
            {
                return Result<ProjectVM?>.NotFound("Project by contract not found");
            }

            return Result<ProjectVM?>.Ok("Projects by contract retrieved", mapper.Map<ProjectVM>(projects));
        }
        catch (Exception exception)
        {
            return Result<ProjectVM?>.InternalError(exception.Message);
        }
    }
}