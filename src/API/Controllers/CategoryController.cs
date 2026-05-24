using API.Controllers.Common;
using BLL;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Category;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = Settings.Roles.AdminRole)]
public class CategoryController(ISender sender)
    : GenericCrudController<int, CategoryVM, CreateCategoryVM, UpdateCategoryVM>(sender)
{
    [AllowAnonymous]
    public override async Task<ActionResult<Result<List<CategoryVM>>>> GetAll(CancellationToken ct)
        => await base.GetAll(ct);

    [AllowAnonymous]
    public override async Task<ActionResult<Result<CategoryVM>>> GetById(int id, CancellationToken ct)
        => await base.GetById(id, ct);
    
    [AllowAnonymous]
    public override async Task<ActionResult<Result<PaginatedItemsVM<CategoryVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await base.GetAllPaginated(pagedVm, ct);
}