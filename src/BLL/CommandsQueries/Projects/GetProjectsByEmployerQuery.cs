using AutoMapper;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.Project;
using MediatR;

namespace BLL.CommandsQueries.Projects;

public record GetProjectsByEmployerQuery : IRequest<Result<List<ProjectVM>?>>;

public class QueryHandler(IProjectQueries projectQueries, IMapper mapper)
    : IRequestHandler<GetProjectsByEmployerQuery, Result<List<ProjectVM>?>>
{
    public async Task<Result<List<ProjectVM>?>> Handle(GetProjectsByEmployerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var projects = await projectQueries.GetByEmployer(cancellationToken);

            return Result<List<ProjectVM>?>.Ok("Projects retrieved", mapper.Map<List<ProjectVM>>(projects));
        }
        catch (Exception exception)
        {
            return Result<List<ProjectVM>?>.InternalError(exception.Message);
        }
    }
}