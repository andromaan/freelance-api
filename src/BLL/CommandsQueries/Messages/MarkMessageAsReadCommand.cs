using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record MarkMessageAsReadCommand(Guid MessageId, Guid SenderId) : IRequest<Result<Guid>>;

public class MarkMessageAsReadCommandHandler(
    IMessageQueries messageQueries,
    IMessageRepository messageRepository) : IRequestHandler<MarkMessageAsReadCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await messageQueries.GetByIdAsync(request.MessageId, cancellationToken);
        
        if (message == null)
            return Result<Guid>.NotFound("Message not found");

        if (message.ReceiverId != request.SenderId)
            return Result<Guid>.Forbidden("You can only mark your received messages as read");

        if (message.IsRead)
            return Result<Guid>.Ok("Message is already read", message.Id);

        message.IsRead = true;
        // Bypassing IUserProvider issues for ModifiedBy since we modified UserProvider to return Guid.Empty for SignalR
        await messageRepository.UpdateAsync(message, cancellationToken);

        return Result<Guid>.Ok("Message marked as read", message.Id);
    }
}
