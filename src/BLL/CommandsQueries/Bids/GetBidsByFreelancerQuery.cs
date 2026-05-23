using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Bids;
using BLL.Services;
using BLL.ViewModels.Bid;
using MediatR;

namespace BLL.CommandsQueries.Bids;

public record GetBidsByFreelancerQuery : IRequest<ServiceResponse>;

public class GetBidsByFreelancerQueryQueryHandler(
    IBidQueries bidQueries,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetBidsByFreelancerQuery, ServiceResponse>
{
    public async Task<ServiceResponse> Handle(GetBidsByFreelancerQuery request,
        CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId(cancellationToken);
        
        var bidsByFreelancer = await bidQueries.GetByFreelancerIdAsync(userId, cancellationToken);
        
        return ServiceResponse.Ok("Bids by freelancer receive successfully",
            mapper.Map<List<BidVM>>(bidsByFreelancer));
    }
}