using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Contracts;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Contract;
using Domain.Models.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ContractController(ISender sender) : BaseController
{
    [Authorize(Roles = Settings.Roles.EmployerRole)]
    [HttpPost("{quoteId:guid}")]
    public async Task<ActionResult> CreateContract([FromRoute] Guid quoteId, CancellationToken ct)
    {
        var command = new CreateContractCommand { QuoteId = quoteId };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpGet("status-enums")]
    public ActionResult GetPlatformsAsync()
    {
        var platforms = Enum.GetValues<ContractStatus>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }

    [Authorize(Roles = Settings.Roles.EmployerRole)]
    [HttpPut]
    public async Task<ActionResult> UpdateContract(Guid contractId, UpdateContractVM vm, CancellationToken ct)
    {
        var command = new Update.Command<UpdateContractVM, Guid, ContractVM> { Id = contractId, Model = vm };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(Roles = Settings.Roles.EmployerRole)]
    [HttpPut("update-status/{contractId:guid}")]
    public async Task<ActionResult> UpdateContractStatus(Guid contractId, UpdateContractStatusVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdateContractStatusVM, Guid, ContractVM> { Id = contractId, Model = vm };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpGet("by-user")]
    public async Task<ActionResult> GetProjectsByEmployer(CancellationToken ct)
    {
        var query = new GetContractByUserQuery();
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("can-contract-be-created/{quoteId:guid}")]
    public async Task<ActionResult> CanContractBeCreated(Guid quoteId, CancellationToken ct)
    {
        var query = new CanContractBeCreatedQuery { QuoteId = quoteId };
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }
    
    [HttpGet("is-exists-by-quote/{quoteId:guid}")]
    public async Task<ActionResult> IsExistsByQuote(Guid quoteId, CancellationToken ct)
    {
        var query = new IsExistsByQuoteQuery { QuoteId = quoteId };
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("completed-by-freelancer-id/{freelancerId:guid}")]
    public async Task<ActionResult> GetProjectsByEmployer(Guid freelancerId, CancellationToken ct)
    {
        var query = new GetContractByFreelancerIdQuery(freelancerId);
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }
}