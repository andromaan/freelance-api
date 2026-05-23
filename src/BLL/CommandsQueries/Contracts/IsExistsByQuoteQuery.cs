using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Projects;
using BLL.Common.Interfaces.Repositories.Quotes;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Contracts;

public record IsExistsByQuoteQuery : IRequest<ServiceResponse<bool>>
{
    public required Guid QuoteId { get; init; }
}

public class IsExistsByQuoteQueryHandler(
    IContractQueries contractQueries,
    IQuoteQueries quoteQueries,
    IProjectQueries projectQueries
)
    : IRequestHandler<IsExistsByQuoteQuery, ServiceResponse<bool>>
{
    public async Task<ServiceResponse<bool>> Handle(IsExistsByQuoteQuery request, CancellationToken cancellationToken)
    {
        var quote = await quoteQueries.GetByIdAsync(request.QuoteId, cancellationToken);
        if (quote is null)
        {
            return ServiceResponse<bool>.NotFound($"Quote with id {request.QuoteId} not found");
        }

        var project = await projectQueries.GetByIdAsync(quote.ProjectId, cancellationToken);

        if (!await contractQueries.IsExistsByQuoteQuery(project!.Id, project.CreatedBy, quote.FreelancerId,
                cancellationToken))
        {
            return ServiceResponse<bool>.Ok("Contract cannot be created. Contract already exists for this quote.");
        }

        return ServiceResponse<bool>.Ok("Contract can be created", true);
    }
}