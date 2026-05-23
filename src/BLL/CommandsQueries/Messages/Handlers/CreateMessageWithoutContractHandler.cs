using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Message;
using Domain.Models.Messaging;

namespace BLL.CommandsQueries.Messages.Handlers;

public class CreateMessageWithoutContractHandler(
    IUserProvider userProvider,
    IUserQueries userQueries
) : ICreateHandler<Message, CreateMessageWithoutContractVM, MessageVM>
{
    public async Task<ServiceResponse<MessageVM?>> HandleAsync(Message entity,
        CreateMessageWithoutContractVM createModel, CancellationToken cancellationToken)
    {
        var senderId = await userProvider.GetUserId();

        var receiver = await userQueries.GetByEmailAsync(createModel.ReceiverEmail, cancellationToken);
        if (receiver == null)
        {
            return ServiceResponse<MessageVM?>.BadRequest("Receiver with the specified email does not exist");
        }

        if (receiver.Id == senderId)
        {
            return ServiceResponse<MessageVM?>.BadRequest("Cannot send a message to yourself");
        }

        entity.ReceiverId = receiver.Id;

        return ServiceResponse<MessageVM?>.Ok(); // Validation passed
    }
}