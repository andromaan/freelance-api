using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Services;
using BLL.ViewModels.Reviews;
using MediatR;

namespace BLL.CommandsQueries.Reviews;

public record GetIsReviewedQuery : IRequest<Result<ReviewVM>>
{
    public Guid ContractId { get; init; }
}

public class GetIsReviewedQueryHandler(
    IMapper mapper,
    IReviewQueries reviewQueries,
    IUserProvider userProvider)
    : IRequestHandler<GetIsReviewedQuery, Result<ReviewVM>>
{
    public async Task<Result<ReviewVM>> Handle(GetIsReviewedQuery request, CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId(cancellationToken);
        var isReviewed = await reviewQueries.GetReviewByContractAndReviewer(userId, request.ContractId, cancellationToken);
        
        try
        {
            return Result<ReviewVM>.Ok("Reviews retrieved", mapper.Map<ReviewVM>(isReviewed));
        }
        catch (Exception exception)
        {
            return Result<ReviewVM>.InternalError(exception.Message);
        }
    }
}