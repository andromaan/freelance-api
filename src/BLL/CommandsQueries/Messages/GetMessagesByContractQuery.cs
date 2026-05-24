using AutoMapper;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record GetMessagesByContractQuery : IRequest<Result<List<MessageVM>?>>
{
    public required Guid ContractId { get; set; }
}

public class GetMessagesByContractQueryHandler(
    IMessageQueries messageQueries,
    IMapper mapper,
    IContractQueries contractQueries)
    : IRequestHandler<GetMessagesByContractQuery, Result<List<MessageVM>?>>
{
    public async Task<Result<List<MessageVM>?>> Handle(GetMessagesByContractQuery request, CancellationToken cancellationToken)
    {
        if (await contractQueries.GetByIdAsync(request.ContractId, cancellationToken) is null)
        {
            return Result<List<MessageVM>?>.NotFound($"Contract with id {request.ContractId} not found");
        }

        try
        {
            var messages = await messageQueries.GetByContractAsync(request.ContractId, cancellationToken);

            return Result<List<MessageVM>?>.Ok("Messages retrieved", mapper.Map<List<MessageVM>>(messages));
        }
        catch (Exception exception)
        {
            return Result<List<MessageVM>?>.InternalError(exception.Message);
        }
    }
}