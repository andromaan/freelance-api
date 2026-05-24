using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Reviews;

public record GetAverageRatingQuery : IRequest<Result<double?>>
{
    public string ReviewedUserEmail { get; init; } = string.Empty;
}

public class GetAverageRatingQueryQueryHandler(IReviewQueries reviewQueries, IUserQueries userQueries)
    : IRequestHandler<GetAverageRatingQuery, Result<double?>>
{
    public async Task<Result<double?>> Handle(GetAverageRatingQuery request, CancellationToken cancellationToken)
    {
        var user = await userQueries.GetByEmailAsync(request.ReviewedUserEmail, cancellationToken);
        if (user is null)
        {
            return Result<double?>.NotFound($"User with email {request.ReviewedUserEmail} not found");
        }

        try
        {
            var reviews = (await reviewQueries.GetReviewsByReviewedUser(user.Id, cancellationToken)).ToList();

            var averageRating = reviews.Any() ? reviews.Average(r => (double)r.Rating) : 0.0;

            return Result<double?>.Ok("Reviews retrieved", averageRating);
        }
        catch (Exception exception)
        {
            return Result<double?>.InternalError(exception.Message);
        }
    }
}