using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Quotes;
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
    public async Task<IActionResult> GetByProjectId(Guid projectId, CancellationToken ct)
    {
        var query = new GetQuotesByProjectIdQuery { ProjectId = projectId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [Authorize(Roles = Settings.Roles.FreelancerRole)]
    [HttpGet("by-freelancer")]
    public async Task<IActionResult> GetByFreelancer(CancellationToken ct)
    {
        var query = new GetQuotesByFreelancerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> GetAll(CancellationToken ct)
        => Task.FromResult<IActionResult>(NotFound());

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<IActionResult> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
        => Task.FromResult<IActionResult>(NotFound());
}