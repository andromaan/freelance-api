using AutoMapper;
using BLL.Common.Interfaces.Repositories.ProjectMilestones;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.ProjectMilestone;
using MediatR;

namespace BLL.CommandsQueries.ProjectMilestones;

public record GetProjectMilestonesByProjectIdQuery : IRequest<Result<List<ProjectMilestoneVM>?>>
{
    public required Guid ProjectId { get; init; }
}

public class QueryHandler(
    IProjectMilestoneQueries projectMilestoneService,
    IProjectQueries projectQueries,
    IMapper mapper)
    : IRequestHandler<GetProjectMilestonesByProjectIdQuery, Result<List<ProjectMilestoneVM>?>>
{
    public async Task<Result<List<ProjectMilestoneVM>?>> Handle(GetProjectMilestonesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingProject = await projectQueries.GetByIdAsync(request.ProjectId, cancellationToken, true);
        if (existingProject == null)
        {
            return Result<List<ProjectMilestoneVM>?>.NotFound($"Project with id {request.ProjectId} not found");
        }

        var result = await projectMilestoneService.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        return Result<List<ProjectMilestoneVM>?>.Ok("Project milestones receive successfully",
            mapper.Map<List<ProjectMilestoneVM>>(result));
    }
}