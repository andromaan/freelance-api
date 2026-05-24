using AutoMapper;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Reviews;
using MediatR;

namespace BLL.CommandsQueries.Reviews;

public record GetByReviewedUserQuery : IRequest<Result<List<ReviewVM>?>>
{
    public string ReviewedUserEmail { get; init; } = string.Empty;
}

public class GetByReviewedUserQueryQueryHandler(IReviewQueries reviewQueries, IUserQueries userQueries, IMapper mapper)
    : IRequestHandler<GetByReviewedUserQuery, Result<List<ReviewVM>?>>
{
    public async Task<Result<List<ReviewVM>?>> Handle(GetByReviewedUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userQueries.GetByEmailAsync(request.ReviewedUserEmail, cancellationToken);
        if (user is null)
        {
            return Result<List<ReviewVM>?>.NotFound($"User with email {request.ReviewedUserEmail} not found");
        }

        try
        {
            var reviews = await reviewQueries.GetReviewsByReviewedUser(user.Id, cancellationToken);

            return Result<List<ReviewVM>?>.Ok("Reviews retrieved", mapper.Map<List<ReviewVM>>(reviews));
        }
        catch (Exception exception)
        {
            return Result<List<ReviewVM>?>.InternalError(exception.Message);
        }
    }
}