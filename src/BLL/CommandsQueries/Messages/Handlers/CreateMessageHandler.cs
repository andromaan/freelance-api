using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Message;
using Domain.Models.Messaging;

namespace BLL.CommandsQueries.Messages.Handlers;

public class CreateMessageHandler(
    IUserQueries userQueries,
    IUserProvider userProvider,
    IContractQueries contractQueries) : ICreateHandler<Message, CreateMessageVM, MessageVM>
{
    public async Task<ServiceResponse<MessageVM?>> HandleAsync(Message? entity, CreateMessageVM createModel,
        CancellationToken cancellationToken)
    {
        var senderId = await userProvider.GetUserId();

        // Перевірка, чи існує контракт
        var contract = await contractQueries.GetByIdAsync(createModel.ContractId, cancellationToken);
        if (contract == null)
        {
            return ServiceResponse<MessageVM?>.BadRequest($"Contract with ID {createModel.ContractId} not found");
        }

        // Перевірка, чи існує одержувач
        var receiver = await userQueries.GetByEmailAsync(createModel.ReceiverEmail, cancellationToken);
        if (receiver == null)
        {
            return ServiceResponse<MessageVM?>.BadRequest("Receiver with the specified email does not exist");
        }

        // Перевірка, чи не намагається користувач відправити повідомлення самому собі
        if (receiver.Id == senderId)
        {
            return ServiceResponse<MessageVM?>.BadRequest("Cannot send a message to yourself");
        }

        entity!.ReceiverId = receiver.Id;

        return ServiceResponse<MessageVM?>.Ok(); // Validation passed
    }
}