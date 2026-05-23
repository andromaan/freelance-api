using AutoMapper;
using BLL.Common.Interfaces.Repositories.ProjectMilestones;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.ProjectMilestone;
using MediatR;

namespace BLL.CommandsQueries.ProjectMilestones;

public record GetProjectMilestonesByProjectIdQuery : IRequest<ServiceResponse<List<ProjectMilestoneVM>?>>
{
    public required Guid ProjectId { get; init; }
}

public class QueryHandler(
    IProjectMilestoneQueries projectMilestoneService,
    IProjectQueries projectQueries,
    IMapper mapper)
    : IRequestHandler<GetProjectMilestonesByProjectIdQuery, ServiceResponse<List<ProjectMilestoneVM>?>>
{
    public async Task<ServiceResponse<List<ProjectMilestoneVM>?>> Handle(GetProjectMilestonesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingProject = await projectQueries.GetByIdAsync(request.ProjectId, cancellationToken, true);
        if (existingProject == null)
        {
            return ServiceResponse<List<ProjectMilestoneVM>?>.NotFound($"Project with id {request.ProjectId} not found");
        }

        var result = await projectMilestoneService.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        return ServiceResponse<List<ProjectMilestoneVM>?>.Ok("Project milestones receive successfully",
            mapper.Map<List<ProjectMilestoneVM>>(result));
    }
}