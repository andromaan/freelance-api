using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.CommandsQueries.Projects;
using BLL.ViewModels;
using BLL.ViewModels.Project;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = Settings.Roles.AdminOrEmployer)]
public class ProjectController(ISender sender)
    : GenericCrudController<Guid, ProjectVM, CreateProjectVM, UpdateProjectVM>(sender)
{
    [HttpPatch("categories/{projectId}")]
    public async Task<IActionResult> UpdateProjectCategories(Guid projectId, [FromBody] UpdateProjectCategoriesVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdateProjectCategoriesVM, Guid, ProjectVM>
        {
            Id = projectId,
            Model = vm
        };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpGet("by-employer")]
    public async Task<IActionResult> GetProjectsByEmployer(CancellationToken ct)
    {
        var query = new GetProjectsByEmployerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [AllowAnonymous]
    public override async Task<IActionResult> GetAll(CancellationToken ct)
        => await base.GetAll(ct);

    [AllowAnonymous]
    public override async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => await base.GetById(id, ct);

    [ApiExplorerSettings(IgnoreApi = true)]
    public override async Task<IActionResult> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await base.GetAllPaginated(pagedVm, ct);

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> GetAllPaginatedFiltered(PagedVM pagedVm, [FromQuery] FilterProjectVM filterProjectVm,
        CancellationToken ct)
    {
        var query = new GetAllFilteredPaginated.Query<FilterProjectVM, ProjectVM>(pagedVm, filterProjectVm);
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
}