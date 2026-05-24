using AutoMapper;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record GetMessagesByUserQuery : IRequest<Result<List<MessageVM>?>>;

public class QueryHandler(IMessageQueries messageQueries, IMapper mapper)
    : IRequestHandler<GetMessagesByUserQuery, Result<List<MessageVM>?>>
{
    public async Task<Result<List<MessageVM>?>> Handle(GetMessagesByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await messageQueries.GetByUserAsync(cancellationToken);

            return Result<List<MessageVM>?>.Ok("Messages retrieved", mapper.Map<List<MessageVM>>(messages));
        }
        catch (Exception exception)
        {
            return Result<List<MessageVM>?>.InternalError(exception.Message);
        }
    }
}