using System.Net;
using BLL.CommandsQueries.Messages;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatController(IMediator mediator) : ControllerBase
{
    [HttpGet("{contractId}")]
    public async Task<IActionResult> GetChatDetails(Guid contractId)
    {
        var query = new GetChatDetailsQuery(contractId);
        var result = await mediator.Send(query);
        
        if (!result.Success)
        {
            if (result.GetStatusCode() == HttpStatusCode.NotFound)
                return NotFound(result.Message);
            return BadRequest(result.Message);
        }
        
        return Ok(result.Data);
    }

    [HttpGet("{contractId}/messages")]
    public async Task<IActionResult> GetMessagesByContractId(Guid contractId)
    {
        var query = new GetMessagesByContractQuery { ContractId = contractId };
        var result = await mediator.Send(query);
        
        if (!result.Success)
        {
            if (result.GetStatusCode() == HttpStatusCode.NotFound)
                return NotFound(result.Message);
            return BadRequest(result.Message);
        }
        
        return Ok(result.Data);
    }
}
