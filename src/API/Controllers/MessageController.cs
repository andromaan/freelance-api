using API.Controllers.Common;
using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.Messages;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Message;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessageController(ISender sender)
    : GenericCrudController<Guid, MessageVM, CreateMessageVM, UpdateMessageVM>(sender)
{
    [HttpPost("without-contract")]
    public async Task<ActionResult<Result<MessageVM>>> CreateWithoutContract(CreateMessageWithoutContractVM vm, CancellationToken ct)
    {
        var command = new Create.Command<CreateMessageWithoutContractVM, MessageVM> { Model = vm };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
    
    [HttpGet("by-user")]
    public async Task<ActionResult<Result<List<MessageVM>>>> GetMessagesByUser(CancellationToken ct)
    {
        var query = new GetMessagesByUserQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }
    
    [HttpGet("by-contract/{contractId}")]
    public async Task<ActionResult<Result<List<MessageVM>>>> GetMessagesByContract(Guid contractId, CancellationToken ct)
    {
        var query = new GetMessagesByContractQuery() { ContractId = contractId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<List<MessageVM>>>> GetAll(CancellationToken ct)
    {
        return base.GetAll(ct);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<PaginatedItemsVM<MessageVM>>>> GetAllPaginated(PagedVM pagedVm, CancellationToken ct)
    {
        return base.GetAllPaginated(pagedVm, ct);
    }
}