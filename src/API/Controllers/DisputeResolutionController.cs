using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.GenericCRUD.Delete;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.GetById;
using BLL.Services;
using BLL.ViewModels.DisputeResolution;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DisputeResolutionController(ISender sender) : BaseController
{
    [HttpGet]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<ActionResult<ServiceResponse<List<DisputeResolutionVM>>>> GetAll(CancellationToken ct)
    {
        var query = new GetAll.Query<DisputeResolutionVM>();
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }
    
    // TODO: make get by user disputeResolution -> dispute -> createdBy
    // [HttpGet("by-user")]
    // public async Task<ActionResult> GetAllByUser(CancellationToken ct)
    // {
    //     var query = new GetDisputesByUserQuery();
    //     var result = await sender.Send(query, ct);
    //     return GetResult(result);
    // }

    [HttpGet("{id}")]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<ActionResult<ServiceResponse<DisputeResolutionVM>>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetById.Query<Guid, DisputeResolutionVM> { Id = id };
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpPost]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<ActionResult<ServiceResponse<DisputeResolutionVM>>> Create([FromBody] CreateDisputeResolutionVM vm, CancellationToken ct)
    {
        var command = new Create.Command<CreateDisputeResolutionVM, DisputeResolutionVM> { Model = vm };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<ActionResult<ServiceResponse<DisputeResolutionVM>>> Delete(Guid id, CancellationToken ct)
    {
        var command = new Delete.Command<DisputeResolutionVM, Guid> { Id = id };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }
}