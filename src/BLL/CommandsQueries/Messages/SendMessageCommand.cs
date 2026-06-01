using AutoMapper;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Messages;
using BLL.Services;
using BLL.ViewModels.Message;
using Domain.Models.Messaging;
using MediatR;

namespace BLL.CommandsQueries.Messages;

public record SendMessageCommand(Guid ContractId, string Text, Guid SenderId) : IRequest<Result<MessageVM>>;

public class SendMessageCommandHandler(
    IContractQueries contractQueries,
    IMessageRepository messageRepository,
    IMapper mapper) : IRequestHandler<SendMessageCommand, Result<MessageVM>>
{
    public async Task<Result<MessageVM>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = request.SenderId;

        var contract = await contractQueries.GetByIdAsync(request.ContractId, cancellationToken);
        if (contract == null)
            return Result<MessageVM>.NotFound("Contract not found");

        if (contract.FreelancerId != senderId && contract.CreatedBy != senderId)
            return Result<MessageVM>.Forbidden("You are not a participant of this contract");

        var receiverId = (contract.FreelancerId == senderId) ? contract.CreatedBy : contract.FreelancerId;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            ReceiverId = receiverId,
            Text = request.Text,
            SentAt = DateTime.UtcNow,
            CreatedBy = senderId
        };

        var createdMessage = await messageRepository.CreateAsync(message, cancellationToken);

        if (createdMessage == null)
            return Result<MessageVM>.InternalError("Failed to save message");

        var vm = mapper.Map<MessageVM>(createdMessage);
        
        // Manual mapping fallback if needed
        if (vm.SenderId == Guid.Empty)
        {
            vm.SenderId = createdMessage.CreatedBy;
            vm.ReceiverId = createdMessage.ReceiverId;
            vm.ContractId = createdMessage.ContractId;
        }

        return Result<MessageVM>.Ok("Message sent", vm);
    }
}
