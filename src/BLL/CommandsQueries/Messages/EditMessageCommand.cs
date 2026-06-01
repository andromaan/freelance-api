using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using BLL.ViewModels.Message;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record EditMessageCommand(Guid MessageId, string NewText, Guid SenderId) : IRequest<Result<MessageVM>>;

public class EditMessageCommandHandler(
    IMessageQueries messageQueries,
    IMessageRepository messageRepository,
    IMapper mapper) : IRequestHandler<EditMessageCommand, Result<MessageVM>>
{
    public async Task<Result<MessageVM>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = request.SenderId;

        var message = await messageQueries.GetByIdAsync(request.MessageId, cancellationToken);
        if (message == null)
            return Result<MessageVM>.NotFound("Message not found");

        if (message.CreatedBy != senderId)
            return Result<MessageVM>.Forbidden("You can only edit your own messages");

        message.Text = request.NewText;
        
        var updatedMessage = await messageRepository.UpdateAsync(message, cancellationToken);

        if (updatedMessage == null)
            return Result<MessageVM>.InternalError("Failed to update message");

        var vm = mapper.Map<MessageVM>(updatedMessage);
        
        if (vm.SenderId == Guid.Empty)
        {
            vm.SenderId = updatedMessage.CreatedBy;
            vm.ReceiverId = updatedMessage.ReceiverId;
            vm.ContractId = updatedMessage.ContractId;
        }

        return Result<MessageVM>.Ok("Message updated", vm);
    }
}
