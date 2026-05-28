using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.CommandsQueries.Projects;
using BLL.Services;
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
    public async Task<ActionResult<Result<ProjectVM>>> UpdateProjectCategories(Guid projectId, [FromBody] UpdateProjectCategoriesVM vm,
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
    public async Task<ActionResult<Result<List<ProjectVM>>>> GetProjectsByEmployer(CancellationToken ct)
    {
        var query = new GetProjectsByEmployerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [AllowAnonymous]
    [HttpGet("by-contract/{contractId:guid}")]
    public async Task<ActionResult<Result<ProjectVM>>> GetProjectByContract(Guid contractId, CancellationToken ct)
    {
        var query = new GetProjectByContractQuery { ContractId = contractId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [AllowAnonymous]
    public override Task<ActionResult<Result<List<ProjectVM>>>> GetAll(CancellationToken ct)
    {
        return base.GetAll(ct);
    }

    [AllowAnonymous]
    public override Task<ActionResult<Result<ProjectVM>>> GetById(Guid id, CancellationToken ct)
    {
        return base.GetById(id, ct);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<PaginatedItemsVM<ProjectVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
    {
        return base.GetAllPaginated(pagedVm, ct);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<Result<PaginatedItemsVM<ProjectVM>>>> GetAllPaginatedFiltered(PagedVM pagedVm, [FromQuery] FilterProjectVM filterProjectVm,
        CancellationToken ct)
    {
        var query = new GetAllFilteredPaginated.Query<FilterProjectVM, ProjectVM>(pagedVm, filterProjectVm);
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
}