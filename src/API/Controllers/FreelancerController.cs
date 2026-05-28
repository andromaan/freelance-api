using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Freelancers;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = Settings.Roles.AdminOrFreelancer)]
public class FreelancerController(ISender sender) : BaseController
{
    [HttpGet]
    public virtual async Task<ActionResult<Result<FreelancerVM>>> GetByUser(CancellationToken ct)
        => GetResult(await sender.Send(new GetFreelancerByUserQuery(), ct));
    
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public virtual async Task<ActionResult<Result<FreelancerVM>>> GetByFreelancerId(Guid id, CancellationToken ct)
        => GetResult(await sender.Send(new GetFreelancerByIdQuery(id), ct));
    
    [AllowAnonymous]
    [HttpGet("{email}")]
    public virtual async Task<ActionResult<Result<FreelancerVM>>> GetByUserEmail(string email, CancellationToken ct)
        => GetResult(await sender.Send(new GetFreelancerByEmailQuery(email), ct));

    [HttpPut]
    public virtual async Task<ActionResult<Result<FreelancerVM>>> Update(UpdateFreelancerVM vm, CancellationToken ct)
        => GetResult(await sender.Send(new UpdateByUser.Command<UpdateFreelancerVM, FreelancerVM> { Model = vm }, ct));

    [HttpPut("skills")]
    public virtual async Task<ActionResult<Result<FreelancerVM>>> UpdateSkills(UpdateFreelancerSkillsVM vm, CancellationToken ct)
        => GetResult(await sender.Send(new UpdateByUser.Command<UpdateFreelancerSkillsVM, FreelancerVM> { Model = vm }, ct));
}