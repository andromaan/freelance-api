using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record CanContractBeCreatedQuery : IRequest<Result<bool>>
{
    public required Guid QuoteId { get; init; }
}

public class CanContractBeCreatedQueryHandler(
    IContractQueries contractQueries,
    IQuoteQueries quoteQueries,
    IProjectQueries projectQueries
)
    : IRequestHandler<CanContractBeCreatedQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(CanContractBeCreatedQuery request, CancellationToken cancellationToken)
    {
        var quote = await quoteQueries.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return Result<bool>.NotFound($"Quote with id {request.QuoteId} not found");
        }

        var project = await projectQueries.GetByIdAsync(quote.ProjectId, cancellationToken);

        if (!await contractQueries.IsContractCanBeCreated(project!.Id, project.CreatedBy, quote.FreelancerId,
                cancellationToken))
        {
            return Result<bool>.Ok("Contract cannot be created. Contract already exists for this quote.");
        }

        return Result<bool>.Ok("Contract can be created", true);
    }
}