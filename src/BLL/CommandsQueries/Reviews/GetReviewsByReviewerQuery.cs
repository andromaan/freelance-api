using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Services;
using BLL.ViewModels.Reviews;
using MediatR;

namespace BLL.CommandsQueries.Reviews;

public record GetByReviewerQuery : IRequest<ServiceResponse<List<ReviewVM>?>>;

public class GetReviewedUserQueryHandler(
    IReviewQueries reviewQueries,
    IMapper mapper,
    IUserProvider userProvider)
    : IRequestHandler<GetByReviewerQuery, ServiceResponse<List<ReviewVM>?>>
{
    public async Task<ServiceResponse<List<ReviewVM>?>> Handle(GetByReviewerQuery request, CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId();

        try
        {
            var reviews = await reviewQueries.GetReviewsByReviewerUser(userId, cancellationToken);

            return ServiceResponse<List<ReviewVM>?>.Ok("Reviews retrieved", mapper.Map<List<ReviewVM>>(reviews));
        }
        catch (Exception exception)
        {
            return ServiceResponse<List<ReviewVM>?>.InternalError(exception.Message);
        }
    }
}