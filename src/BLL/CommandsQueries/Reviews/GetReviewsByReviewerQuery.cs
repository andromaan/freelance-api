using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Services;
using BLL.ViewModels.Reviews;
using MediatR;

namespace BLL.CommandsQueries.Reviews;

public record GetByReviewerQuery : IRequest<Result<List<ReviewVM>?>>;

public class GetReviewedUserQueryHandler(
    IReviewQueries reviewQueries,
    IMapper mapper,
    IUserProvider userProvider)
    : IRequestHandler<GetByReviewerQuery, Result<List<ReviewVM>?>>
{
    public async Task<Result<List<ReviewVM>?>> Handle(GetByReviewerQuery request, CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId();

        try
        {
            var reviews = await reviewQueries.GetReviewsByReviewerUser(userId, cancellationToken);

            return Result<List<ReviewVM>?>.Ok("Reviews retrieved", mapper.Map<List<ReviewVM>>(reviews));
        }
        catch (Exception exception)
        {
            return Result<List<ReviewVM>?>.InternalError(exception.Message);
        }
    }
}