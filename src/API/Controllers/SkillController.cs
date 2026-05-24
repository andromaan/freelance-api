using API.Controllers.Common;
using BLL;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Skill;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = Settings.Roles.AdminRole)]
public class SkillController(ISender sender)
    : GenericCrudController<int, SkillVM, CreateSkillVM, UpdateSkillVM>(sender)
{
    [AllowAnonymous]
    public override async Task<ActionResult<Result<List<SkillVM>>>> GetAll(CancellationToken ct)
        => await base.GetAll(ct);

    [AllowAnonymous]
    public override async Task<ActionResult<Result<SkillVM>>> GetById(int id, CancellationToken ct)
        => await base.GetById(id, ct);

    [AllowAnonymous]
    public override async Task<ActionResult<Result<PaginatedItemsVM<SkillVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await base.GetAllPaginated(pagedVm, ct);
}