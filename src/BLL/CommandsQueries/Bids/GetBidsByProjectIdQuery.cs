using AutoMapper;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Services;
using BLL.ViewModels.Bid;
using MediatR;

namespace BLL.CommandsQueries.Bids;

public record GetBidsByProjectIdQuery : IRequest<Result<List<BidVM>?>>
{
    public required Guid ProjectId { get; init; }
}

public class QueryHandler(
    IBidQueries bidQueries,
    IProjectQueries projectQueries,
    IMapper mapper)
    : IRequestHandler<GetBidsByProjectIdQuery, Result<List<BidVM>?>>
{
    public async Task<Result<List<BidVM>?>> Handle(GetBidsByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingProject = await projectQueries.GetByIdAsync(request.ProjectId, cancellationToken, true);
        if (existingProject == null)
        {
            return Result<List<BidVM>?>.NotFound($"Project with id {request.ProjectId} not found");
        }

        var result = await bidQueries.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        return Result<List<BidVM>?>.Ok("Bids receive successfully",
            mapper.Map<List<BidVM>>(result));
    }
}