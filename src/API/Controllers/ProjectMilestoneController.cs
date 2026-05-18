using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.ProjectMilestones;
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
    public async Task<IActionResult> GetByProjectId(Guid projectId, CancellationToken ct)
    {
        var query = new GetProjectMilestonesByProjectIdQuery { ProjectId = projectId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> GetAll(CancellationToken ct)
        => Task.FromResult<IActionResult>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => Task.FromResult<IActionResult>(NotFound());
}