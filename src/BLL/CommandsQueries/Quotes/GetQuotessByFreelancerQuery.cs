using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using BLL.ViewModels.Quote;
using MediatR;

namespace BLL.CommandsQueries.Quotes;

public record GetQuotesByFreelancerQuery : IRequest<ServiceResponse<List<QuoteVM>?>>;

public class GetQuotesByFreelancerQueryQueryHandler(
    IQuoteQueries quoteQueries,
    IUserProvider userProvider,
    IMapper mapper)
    : IRequestHandler<GetQuotesByFreelancerQuery, ServiceResponse<List<QuoteVM>?>>
{
    public async Task<ServiceResponse<List<QuoteVM>?>> Handle(GetQuotesByFreelancerQuery request,
        CancellationToken cancellationToken)
    {
        var userId = await userProvider.GetUserId(cancellationToken);
        
        var quotesByFreelancer = await quoteQueries.GetByFreelancerIdAsync(userId, cancellationToken);
        
        return ServiceResponse<List<QuoteVM>?>.Ok("Quotes by freelancer receive successfully",
            mapper.Map<List<QuoteVM>>(quotesByFreelancer));
    }
}