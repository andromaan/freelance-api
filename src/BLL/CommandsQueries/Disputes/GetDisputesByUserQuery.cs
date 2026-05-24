using AutoMapper;
using BLL.Common.Interfaces.Repositories.Disputes;
using BLL.Services;
using BLL.ViewModels.Dispute;
using MediatR;

namespace BLL.CommandsQueries.Disputes;

public record GetDisputesByUserQuery : IRequest<Result<List<DisputeVM>?>>;

public class QueryHandler(IDisputeQueries disputeQueries, IMapper mapper)
    : IRequestHandler<GetDisputesByUserQuery, Result<List<DisputeVM>?>>
{
    public async Task<Result<List<DisputeVM>?>> Handle(GetDisputesByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var disputes = await disputeQueries.GetDisputesByUser(cancellationToken);

            return Result<List<DisputeVM>?>.Ok("Disputes retrieved", mapper.Map<List<DisputeVM>>(disputes));
        }
        catch (Exception exception)
        {
            return Result<List<DisputeVM>?>.InternalError(exception.Message);
        }
    }
}