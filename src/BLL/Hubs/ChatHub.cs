using BLL.CommandsQueries.Messages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BLL.Hubs;

[Authorize]
public class ChatHub(IMediator mediator) : Hub
{
    public async Task JoinChat(Guid contractId)
    {
        // Add the user connection to the group corresponding to this chat (contractId)
        await Groups.AddToGroupAsync(Context.ConnectionId, contractId.ToString());
    }

    public async Task LeaveChat(Guid contractId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, contractId.ToString());
    }

    public async Task SendMessage(Guid contractId, string text)
    {
        var senderId = Guid.Parse(Context.UserIdentifier!);
        var command = new SendMessageCommand(contractId, text, senderId);
        var result = await mediator.Send(command);

        if (result.Success && result.Data != null)
        {
            // Broadcast the new message to everyone in the chat group
            await Clients.Group(contractId.ToString()).SendAsync("ReceiveMessage", result.Data);
        }
        else
        {
            // Optionally, send an error back to the caller
            await Clients.Caller.SendAsync("ErrorMessage", result.Message);
        }
    }

    public async Task EditMessage(Guid contractId, Guid messageId, string newText)
    {
        var senderId = Guid.Parse(Context.UserIdentifier!);
        var command = new EditMessageCommand(messageId, newText, senderId);
        var result = await mediator.Send(command);

        if (result.Success && result.Data != null)
        {
            // Broadcast the updated message
            await Clients.Group(contractId.ToString()).SendAsync("MessageEdited", result.Data);
        }
        else
        {
            await Clients.Caller.SendAsync("ErrorMessage", result.Message);
        }
    }

    public async Task DeleteMessage(Guid contractId, Guid messageId)
    {
        var senderId = Guid.Parse(Context.UserIdentifier!);
        var command = new DeleteMessageCommand(messageId, senderId);
        var result = await mediator.Send(command);

        if (result.Success)
        {
            // Broadcast the deleted message ID
            await Clients.Group(contractId.ToString()).SendAsync("MessageDeleted", messageId);
        }
        else
        {
            await Clients.Caller.SendAsync("ErrorMessage", result.Message);
        }
    }
}
