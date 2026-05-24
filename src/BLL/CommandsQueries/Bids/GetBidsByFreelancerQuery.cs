using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Services;
using BLL.ViewModels.Bid;
using MediatR;

namespace BLL.CommandsQueries.Bids;

public record GetBidsByFreelancerQuery : IRequest<Result<List<BidVM>?>>;

public class GetBidsByFreelancerQueryQueryHandler(
    IBidQueries bidQueries,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetBidsByFreelancerQuery, Result<List<BidVM>?>>
{
    public async Task<Result<List<BidVM>?>> Handle(GetBidsByFreelancerQuery request,
        CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId(cancellationToken);
        
        var bidsByFreelancer = await bidQueries.GetByFreelancerIdAsync(userId, cancellationToken);
        
        return Result<List<BidVM>?>.Ok("Bids by freelancer receive successfully",
            mapper.Map<List<BidVM>>(bidsByFreelancer));
    }
}