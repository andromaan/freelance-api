using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record DeleteMessageCommand(Guid MessageId, Guid SenderId) : IRequest<Result<Guid>>;

public class DeleteMessageCommandHandler(
    IMessageQueries messageQueries,
    IMessageRepository messageRepository) : IRequestHandler<DeleteMessageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = request.SenderId;

        var message = await messageQueries.GetByIdAsync(request.MessageId, cancellationToken);
        if (message == null)
            return Result<Guid>.NotFound("Message not found");

        if (message.CreatedBy != senderId)
            return Result<Guid>.Forbidden("You can only delete your own messages");

        await messageRepository.DeleteAsync(request.MessageId, cancellationToken);

        return Result<Guid>.Ok("Message deleted", request.MessageId);
    }
}
