using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Quotes;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Quote;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class QuoteController(ISender sender)
    : GenericCrudController<Guid, QuoteVM, CreateQuoteVM, UpdateQuoteVM>(sender)
{
    [HttpGet("by-project/{projectId}")]
    public async Task<ActionResult<Result<List<QuoteVM>>>> GetByProjectId(Guid projectId, CancellationToken ct)
    {
        var query = new GetQuotesByProjectIdQuery { ProjectId = projectId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [Authorize(Roles = Settings.Roles.FreelancerRole)]
    [HttpGet("by-freelancer")]
    public async Task<ActionResult<Result<List<QuoteVM>>>> GetByFreelancer(CancellationToken ct)
    {
        var query = new GetQuotesByFreelancerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<List<QuoteVM>>>> GetAll(CancellationToken ct)
        => Task.FromResult<ActionResult<Result<List<QuoteVM>>>>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<PaginatedItemsVM<QuoteVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => Task.FromResult<ActionResult<Result<PaginatedItemsVM<QuoteVM>>>>(NotFound());
}