using AutoMapper;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.Project;
using MediatR;

namespace BLL.CommandsQueries.Projects;

public record GetProjectsByEmployerQuery : IRequest<ServiceResponse<List<ProjectVM>?>>;

public class QueryHandler(IProjectQueries projectQueries, IMapper mapper)
    : IRequestHandler<GetProjectsByEmployerQuery, ServiceResponse<List<ProjectVM>?>>
{
    public async Task<ServiceResponse<List<ProjectVM>?>> Handle(GetProjectsByEmployerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var projects = await projectQueries.GetByEmployer(cancellationToken);

            return ServiceResponse<List<ProjectVM>?>.Ok("Projects retrieved", mapper.Map<List<ProjectVM>>(projects));
        }
        catch (Exception exception)
        {
            return ServiceResponse<List<ProjectVM>?>.InternalError(exception.Message);
        }
    }
}