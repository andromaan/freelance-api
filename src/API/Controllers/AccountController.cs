using API.Controllers.Common;
using BLL.CommandsQueries.Auth;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Auth;
using BLL.ViewModels.Project;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController(ISender sender) : BaseController
{
    [HttpPost("sign-up")]
    public async Task<ActionResult<ServiceResponse<JwtVM>>> SignUp([FromBody] SignUpVM vm, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new SignUpCommand(vm), cancellationToken);
        return GetResult(response);
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<ServiceResponse<ProjectVM>>> SignIn([FromBody] SignInVM vm, CancellationToken cancellationToken)
    {
        var response = await sender.Send(new SignInCommand(vm), cancellationToken);
        return GetResult(response);
    }
    
    [HttpPost("external-login")]
    public async Task<ActionResult> GoogleExternalLoginAsync([FromBody] ExternalLoginVM model,
        CancellationToken cancellationToken)
    {
        var command = new GoogleExternalLoginCommand { Model = model };
        var result = await sender.Send(command, cancellationToken);
        return GetResult(result);
    }
}