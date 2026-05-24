using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Notifications;

public class MarkNotificationAsRead
{
    public record Command(Guid NotificationId) : IRequest<Result<string>>;

    public class CommandHandler(
        INotificationRepository repository,
        IUserProvider userProvider) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await userProvider.GetUserId(cancellationToken);

            var notification = await repository.MarkAsReadAsync(request.NotificationId, userId, cancellationToken);

            return notification is null
                ? Result<string>.NotFound($"Notification {request.NotificationId} not found or it's system notification.")
                : Result<string>.Ok();
        }
    }
}

