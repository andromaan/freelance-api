using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.ContractMilestones;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.ContractMilestone;
using Domain.Models.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ContractMilestoneController(ISender sender)
    : GenericCrudController<Guid, ContractMilestoneVM, CreateContractMilestoneVM, UpdateContractMilestoneVM>(sender)
{
    [AllowAnonymous]
    [HttpGet("by-contract/{contractId}")]
    public async Task<ActionResult<Result<List<ContractMilestoneVM>>>> GetByContractId(Guid contractId, CancellationToken ct)
    {
        var query = new GetContractMilestonesByContractIdQuery { ContractId = contractId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [HttpGet("milestone-status-enums")]
    public ActionResult GetPlatformsAsync()
    {
        var platforms = Enum.GetValues<ContractMilestoneStatus>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [HttpGet("status-freelancer-enums")]
    public ActionResult GetFreelancerStatusEnumsAsync()
    {
        var platforms = Enum.GetValues<ContractMilestoneFreelancerStatus>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [Authorize(Roles = Settings.Roles.FreelancerRole)]
    [HttpPut("status/{id:guid}/freelancer")]
    public async Task<ActionResult<Result<ContractMilestoneVM>>> UpdateContractMilestoneStatusForFreelancer(
        Guid id,
        [FromBody] UpdContractMilestoneStatusFreelancerVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdContractMilestoneStatusFreelancerVM, Guid, ContractMilestoneVM>
        {
            Id = id,
            Model = vm
        };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [HttpGet("status-employer-enums")]
    public ActionResult GetEmployerStatusEnumsAsync()
    {
        var platforms = Enum.GetValues<ContractMilestoneEmployerStatus>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [Authorize(Roles = Settings.Roles.EmployerRole)]
    [HttpPut("status/{id:guid}/employer")]
    public async Task<ActionResult<Result<ContractMilestoneVM>>> UpdateContractMilestoneStatusForEmployer(
        Guid id,
        [FromBody] UpdContractMilestoneStatusEmployerVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdContractMilestoneStatusEmployerVM, Guid, ContractMilestoneVM>
        {
            Id = id,
            Model = vm
        };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    [HttpGet("status-moderator-enums")]
    public ActionResult GetModeratorStatusEnumsAsync()
    {
        var platforms = Enum.GetValues<ContractMilestoneStatus>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [Authorize(Policy = Settings.Roles.AdminOrModerator)]
    [HttpPut("status/{id:guid}/moderator")]
    public async Task<ActionResult<Result<ContractMilestoneVM>>> UpdateContractMilestoneStatusForModerator(
        Guid id,
        [FromBody] UpdContractMilestoneStatusModeratorVM vm,
        CancellationToken ct)
    {
        var command = new Update.Command<UpdContractMilestoneStatusModeratorVM, Guid, ContractMilestoneVM>
        {
            Id = id,
            Model = vm
        };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<List<ContractMilestoneVM>>>> GetAll(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<PaginatedItemsVM<ContractMilestoneVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}