using API.Controllers.Common;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.Notifications;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Notification;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationController(ISender sender) : BaseController
{
    [HttpGet("is-not-read")]
    public async Task<ActionResult<Result<List<NotificationVM>>>> GetAll(CancellationToken ct)
    {
        var query = new GetAll.Query<NotificationVM>();
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<Result<PaginatedItemsVM<NotificationVM>>>> GetAllPaginated([FromQuery] PagedVM pagedVm, CancellationToken ct)
    {
        var query = new GetAllPaginated.Query<NotificationVM>(pagedVm);
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("type-employer-enums")]
    public ActionResult GetEmployerTypesEnumsAsync()
    {
        var platforms = Enum.GetValues<NotificationTypeEmployer>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }

    [HttpGet("type-freelancer-enums")]
    public ActionResult GetFreelancerTypesEnumsAsync()
    {
        var platforms = Enum.GetValues<NotificationTypeFreelancer>()
            .Select(x => new { Name = x.ToString(), Value = (int)x })
            .ToList();

        return Ok(platforms);
    }
    
    [HttpGet("filtered")]
    public async Task<ActionResult<Result<PaginatedItemsVM<NotificationVM>>>> GetAllFiltered([FromQuery] PagedVM pagedVm,
        [FromQuery] FilterNotificationVM filterVm, CancellationToken ct)
    {
        var query = new GetAllFilteredPaginated.Query<FilterNotificationVM, NotificationVM>(pagedVm, filterVm);
        var result = await sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpPatch("{id:guid}/toggle-read")]
    public async Task<ActionResult<Result<NotificationVM>>> ToggleRead(Guid id, CancellationToken ct)
    {
        var command = new MarkNotificationAsRead.Command(id);
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<Result<List<NotificationVM>>>> MarkAllRead(CancellationToken ct)
    {
        var command = new MarkAllNotificationsAsRead.Command();
        var result = await sender.Send(command, ct);
        return GetResult(result);
    }
}