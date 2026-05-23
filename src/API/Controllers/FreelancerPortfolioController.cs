using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Portfolios;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Portfolio;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = Settings.Roles.FreelancerRole)]
public class FreelancerPortfolioController(ISender sender)
    : GenericCrudController<Guid, PortfolioVM, CreatePortfolioVM, UpdatePortfolioVM>(sender)
{
    [AllowAnonymous]
    [HttpGet("get-by-freelancer/{freelancerId:guid}")]
    public virtual async Task<ActionResult<ServiceResponse<List<PortfolioVM>>>> GetByUser(Guid freelancerId, CancellationToken ct)
        => GetResult(await Sender.Send(new GetPortfoliosByFreelancerIdQuery(freelancerId), ct));
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<ServiceResponse<List<PortfolioVM>>>> GetAll(CancellationToken ct)
        => Task.FromResult<ActionResult<ServiceResponse<List<PortfolioVM>>>>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<ServiceResponse<PaginatedItemsVM<PortfolioVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => Task.FromResult<ActionResult<ServiceResponse<PaginatedItemsVM<PortfolioVM>>>>(NotFound());
}