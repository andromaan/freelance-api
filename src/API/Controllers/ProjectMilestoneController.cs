using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.ProjectMilestones;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.ProjectMilestone;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = Settings.Roles.AdminOrEmployer)]
public class ProjectMilestoneController(ISender sender)
    : GenericCrudController<Guid, ProjectMilestoneVM, CreateProjectMilestoneVM, UpdateProjectMilestoneVM>(sender)
{
    [AllowAnonymous]
    [HttpGet("by-project/{projectId}")]
    public async Task<ActionResult<ServiceResponse<List<ProjectMilestoneVM>>>> GetByProjectId(Guid projectId,
        CancellationToken ct)
    {
        var query = new GetProjectMilestonesByProjectIdQuery { ProjectId = projectId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<ServiceResponse<List<ProjectMilestoneVM>>>> GetAll(CancellationToken ct)
        => Task.FromResult<ActionResult<ServiceResponse<List<ProjectMilestoneVM>>>>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<ServiceResponse<PaginatedItemsVM<ProjectMilestoneVM>>>> GetAllPaginated(
        PagedVM pagedVm, CancellationToken ct)
        => Task.FromResult<ActionResult<ServiceResponse<PaginatedItemsVM<ProjectMilestoneVM>>>>(NotFound());
}