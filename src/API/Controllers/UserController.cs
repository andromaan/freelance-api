using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.GenericCRUD.GetAll;
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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController(ISender sender)
    : GenericCrudController<Guid, UserVM, CreateUserByAdminVM, UpdateUserByAdminVM>(sender)
{
    [HttpGet("roles")]
    public async Task<ActionResult> GetRoles(CancellationToken ct)
    {
        var command = new GetAll.Query<RoleVM>();
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [HttpGet("get-myself")]
    public async Task<ActionResult> GetMyself(CancellationToken ct)
    {
        var command = new GetUserByTokenQuery();
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpPatch("update-avatar")]
    public async Task<ActionResult> UpdateAvatar(IFormFile file, CancellationToken ct)
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

    [HttpPost("languages")]
    public async Task<ActionResult> CreateLanguage(CreateUserLanguageVM vm, CancellationToken ct)
    {
        var command = new CreateUserLanguageCommand(vm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [HttpPut("languages")]
    public async Task<ActionResult> UpdateLanguage(UpdateUserLanguageVM vm, CancellationToken ct)
    {
        var command = new UpdateUserLanguageCommand(vm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [HttpDelete("languages/{languageId}")]
    public async Task<ActionResult> DeleteLanguage(int languageId, CancellationToken ct)
    {
        var command = new DeleteUserLanguageCommand(languageId);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override async Task<ActionResult> Create(CreateUserByAdminVM byAdminVm, CancellationToken ct)
    {
        var command = new CreateUserByAdminCommand(byAdminVm);
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult> Update(Guid id, UpdateUserByAdminVM byAdminVm, CancellationToken ct)
        => base.Update(id, byAdminVm, ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => base.Delete(id, ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult> GetAll(CancellationToken ct)
        => base.GetAll(ct);

    [Authorize(Roles = Settings.Roles.AdminRole)]
    public override Task<ActionResult> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
    => base.GetAllPaginated(pagedVm, ct);
}