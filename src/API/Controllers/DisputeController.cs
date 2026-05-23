using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Disputes;
using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.GenericCRUD.Delete;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.GetById;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Dispute;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DisputeController(ISender sender) : BaseController
{
    [HttpGet]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var query = new GetAll.Query<DisputeVM>();
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }
    
    [HttpGet("by-user")]
    public async Task<IActionResult> GetAllByUser(CancellationToken ct)
    {
        var query = new GetDisputesByUserQuery();
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetById.Query<Guid, DisputeVM> { Id = id };
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDisputeVM vm, CancellationToken ct)
    {
        var command = new Create.Command<CreateDisputeVM, DisputeVM> { Model = vm };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new Delete.Command<DisputeVM, Guid> { Id = id };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    [HttpGet("status-moderator-enums")]
    public IActionResult GetModeratorStatusEnumsAsync()
    {
        var platforms = Enum.GetValues<DisputeStatusForModerator>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [HttpPut("{id}/status")]
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDisputeStatusForModeratorVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdateDisputeStatusForModeratorVM, Guid, DisputeVM> { Id = id, Model = vm };
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }
}