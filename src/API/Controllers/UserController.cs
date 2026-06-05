using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Services;
using BLL.CommandsQueries.UserLanguages;
using BLL.CommandsQueries.Users;
using BLL.ViewModels;
using BLL.ViewModels.Roles;
using BLL.ViewModels.User;
using BLL.ViewModels.UserLanguage;
using Domain.Models.Users;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController(ISender sender)
    : GenericCrudController<Guid, UserVM, CreateUserByAdminVM, UpdateUserByAdminVM>(sender)
{
    [HttpGet("roles")]
    public async Task<ActionResult<Result<List<RoleVM>>>> GetRoles(CancellationToken ct)
    {
        var command = new GetAll.Query<RoleVM>();
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("get-myself")]
    public async Task<ActionResult<Result<UserVM>>> GetMyself(CancellationToken ct)
    {
        var command = new GetUserByTokenQuery();
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPatch("update-avatar")]
    public async Task<ActionResult<Result<UserVM>>> UpdateAvatar(IFormFile file, CancellationToken ct)
    {
        var command = new UpdateUserAvatarCommand(file);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpGet("proficiency-levels")]
    public ActionResult GetProficiencyLevelsAsync()
    {
        var platforms = Enum.GetValues<ProficiencyLevel>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("languages")]
    public async Task<ActionResult<Result<UserLanguageVM>>> CreateLanguage(CreateUserLanguageVM vm,
        CancellationToken ct)
    {
        var command = new CreateUserLanguageCommand(vm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("languages")]
    public async Task<ActionResult<Result<UserLanguageVM>>> UpdateLanguage(UpdateUserLanguageVM vm,
        CancellationToken ct)
    {
        var command = new UpdateUserLanguageCommand(vm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("languages/{languageId}")]
    public async Task<ActionResult<Result<UserLanguageVM>>> DeleteLanguage(int languageId, CancellationToken ct)
    {
        var command = new DeleteUserLanguageCommand(languageId);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut]
    public virtual async Task<ActionResult<Result<UserVM>>> Update(UpdateUserVM vm, CancellationToken ct)
        => GetResult(await Sender.Send(new UpdateByUser.Command<UpdateUserVM, UserVM> { Model = vm }, ct));

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override async Task<ActionResult<Result<UserVM>>> Create(CreateUserByAdminVM byAdminVm, CancellationToken ct)
    {
        var command = new CreateUserByAdminCommand(byAdminVm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult<Result<UserVM>>> Update(Guid id, UpdateUserByAdminVM byAdminVm,
        CancellationToken ct)
        => base.Update(id, byAdminVm, ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => base.Delete(id, ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult<Result<List<UserVM>>>> GetAll(CancellationToken ct)
        => base.GetAll(ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult<Result<PaginatedItemsVM<UserVM>>>> GetAllPaginated(PagedVM pagedVm,
        CancellationToken ct)
        => base.GetAllPaginated(pagedVm, ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    [HttpGet("search")]
    public async Task<ActionResult<Result<PaginatedItemsVM<UserVM>>>> GetAllPaginatedFiltered([FromQuery] PagedVM pagedVm, [FromQuery] FilterUserVM filterVm, CancellationToken ct)
    {
        var query = new BLL.CommandsQueries.Users.GetFiltered.GetFilteredUsers.Query(pagedVm, filterVm);
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
}