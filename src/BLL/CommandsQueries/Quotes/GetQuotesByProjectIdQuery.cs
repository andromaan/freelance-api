using AutoMapper;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using BLL.ViewModels.Quote;
using MediatR;

namespace BLL.CommandsQueries.Quotes;

public record GetQuotesByProjectIdQuery : IRequest<ServiceResponse<List<QuoteVM>?>>
{
    public required Guid ProjectId { get; init; }
}

public class QueryHandler(
    IQuoteQueries quoteQueries,
    IProjectQueries projectQueries,
    IMapper mapper)
    : IRequestHandler<GetQuotesByProjectIdQuery, ServiceResponse<List<QuoteVM>?>>
{
    public async Task<ServiceResponse<List<QuoteVM>?>> Handle(GetQuotesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingProject = await projectQueries.GetByIdAsync(request.ProjectId, cancellationToken, true);
        if (existingProject == null)
        {
            return ServiceResponse<List<QuoteVM>?>.NotFound($"Project with id {request.ProjectId} not found");
        }

        var result = await quoteQueries.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        return ServiceResponse<List<QuoteVM>?>.Ok("Quotes receive successfully",
            mapper.Map<List<QuoteVM>>(result));
    }
}