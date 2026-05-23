using AutoMapper;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record GetMessagesByUserQuery : IRequest<ServiceResponse<List<MessageVM>?>>;

public class QueryHandler(IMessageQueries messageQueries, IMapper mapper)
    : IRequestHandler<GetMessagesByUserQuery, ServiceResponse<List<MessageVM>?>>
{
    public async Task<ServiceResponse<List<MessageVM>?>> Handle(GetMessagesByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await messageQueries.GetByUserAsync(cancellationToken);

            return ServiceResponse<List<MessageVM>?>.Ok("Messages retrieved", mapper.Map<List<MessageVM>>(messages));
        }
        catch (Exception exception)
        {
            return ServiceResponse<List<MessageVM>?>.InternalError(exception.Message);
        }
    }
}