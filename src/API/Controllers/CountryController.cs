using API.Controllers.Common;
using BLL;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Country;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = Settings.Roles.AdminRole)]
public class CountryController(ISender sender)
    : GenericCrudController<int, CountryVM, CreateCountryVM, UpdateCountryVM>(sender)
{
    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<List<CountryVM>>>> GetAll(CancellationToken ct)
        => await base.GetAll(ct);

    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<CountryVM>>> GetById(int id, CancellationToken ct)
        => await base.GetById(id, ct);

    [AllowAnonymous]
    public override async Task<ActionResult<ServiceResponse<PaginatedItemsVM<CountryVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await base.GetAllPaginated(pagedVm, ct);
}