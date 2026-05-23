using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Bids;
using BLL.ViewModels;
using BLL.ViewModels.Bid;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BidController(ISender sender)
    : GenericCrudController<Guid, BidVM, CreateBidVM, UpdateBidVM>(sender)
{
    // TODO Продумати логіку доступу роботодавців і чи можуть не авторизовані користувачі бачити заявки
    [AllowAnonymous]
    [HttpGet("by-project/{projectId}")]
    public async Task<ActionResult> GetByProjectId(Guid projectId, CancellationToken ct)
    {
        var query = new GetBidsByProjectIdQuery { ProjectId = projectId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [Authorize(Roles = Settings.Roles.FreelancerRole)]
    [HttpGet("by-freelancer")]
    public async Task<ActionResult> GetByFreelancer(CancellationToken ct)
    {
        var query = new GetBidsByFreelancerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    
    [Authorize(Policy = Settings.Roles.AdminOrEmployer)]
    [HttpPatch("is-interesting/{id}")]
    public async Task<ActionResult> UpdateIsInteresting(Guid id, bool isInteresting, CancellationToken ct)
    {
        var command = new UpdateBidInterestingCommand
        {
            BidId = id,
            IsInteresting = isInteresting
        };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [AllowAnonymous]
    public override async Task<ActionResult> GetById(Guid id, CancellationToken ct)
        => await base.GetById(id, ct);

    [ApiExplorerSettings(IgnoreApi = true)]
    public override async Task<ActionResult> GetAll(CancellationToken ct)
        => await Task.FromResult<ActionResult>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override async Task<ActionResult> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => await Task.FromResult<ActionResult>(NotFound());
}