using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Employers;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Services;
using BLL.ViewModels.Employer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = Settings.Roles.AdminOrEmployer)]
public class EmployerController(ISender sender) : BaseController
{
    [HttpGet]
    public virtual async Task<ActionResult<Result<EmployerVM>>> GetByUser(CancellationToken ct)
        => GetResult(await sender.Send(new GetEmployerByUserQuery(), ct));

    [HttpPut]
    public virtual async Task<ActionResult<Result<EmployerVM>>> Update(UpdateEmployerVM vm, CancellationToken ct)
        => GetResult(await sender.Send(new UpdateByUser.Command<UpdateEmployerVM, EmployerVM> { Model = vm }, ct));
}