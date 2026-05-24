using AutoMapper;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using BLL.ViewModels.Quote;
using MediatR;

namespace BLL.CommandsQueries.Quotes;

public record GetQuotesByProjectIdQuery : IRequest<Result<List<QuoteVM>?>>
{
    public required Guid ProjectId { get; init; }
}

public class QueryHandler(
    IQuoteQueries quoteQueries,
    IProjectQueries projectQueries,
    IMapper mapper)
    : IRequestHandler<GetQuotesByProjectIdQuery, Result<List<QuoteVM>?>>
{
    public async Task<Result<List<QuoteVM>?>> Handle(GetQuotesByProjectIdQuery request,
        CancellationToken cancellationToken)
    {
        var existingProject = await projectQueries.GetByIdAsync(request.ProjectId, cancellationToken, true);
        if (existingProject == null)
        {
            return Result<List<QuoteVM>?>.NotFound($"Project with id {request.ProjectId} not found");
        }

        var result = await quoteQueries.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        return Result<List<QuoteVM>?>.Ok("Quotes receive successfully",
            mapper.Map<List<QuoteVM>>(result));
    }
}