using API.Controllers.Common;
using BLL;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Language;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = Settings.Roles.AdminRole)]
public class LanguageController(ISender sender)
    : GenericCrudController<int, LanguageVM, CreateLanguageVM, UpdateLanguageVM>(sender)
{
    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<List<LanguageVM>>>> GetAll(CancellationToken ct)
        => await base.GetAll(ct);

    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<LanguageVM>>> GetById(int id, CancellationToken ct)
        => await base.GetById(id, ct);

    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<PaginatedItemsVM<LanguageVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await base.GetAllPaginated(pagedVm, ct);
}