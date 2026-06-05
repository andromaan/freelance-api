using API.Controllers.Common;
using BLL;
using BLL.CommandsQueries.Reviews;
using BLL.Services;
using BLL.ViewModels.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Policy = Settings.Roles.AnyAuthenticated)]
public class ReviewController(ISender sender)
    : GenericCrudController<Guid, ReviewVM, CreateReviewVM, UpdateReviewVM>(sender)
{
    [AllowAnonymous]
    [HttpGet("by-reviewed-user/{reviewedUserEmail}")]
    public async Task<ActionResult<Result<List<ReviewVM>>>> GetByGetReviewedUser(string reviewedUserEmail,
        CancellationToken ct)
    {
        var query = new GetByReviewedUserQuery { ReviewedUserEmail = reviewedUserEmail };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [AllowAnonymous]
    [HttpGet("average-rating/{reviewedUserEmail}")]
    public async Task<ActionResult<Result<double>>> GetAverageRating(string reviewedUserEmail, CancellationToken ct)
    {
        var query = new GetAverageRatingQuery { ReviewedUserEmail = reviewedUserEmail };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("by-user")]
    public async Task<ActionResult<Result<List<ReviewVM>>>> GetByGetReviewer(CancellationToken ct)
    {
        var query = new GetByReviewerQuery();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("is-reviewed/{contractId:guid}")]
    public async Task<ActionResult<Result<ReviewVM>>> IsReviewed(Guid contractId, CancellationToken ct)
    {
        var query = new GetIsReviewedQuery { ContractId = contractId };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override Task<ActionResult<Result<List<ReviewVM>>>> GetAll(CancellationToken ct)
        => Task.FromResult<ActionResult<Result<List<ReviewVM>>>>(NotFound());
}