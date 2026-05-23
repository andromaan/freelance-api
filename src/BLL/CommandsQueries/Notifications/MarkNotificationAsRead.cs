using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Notifications;

public class MarkNotificationAsRead
{
    public record Command(Guid NotificationId) : IRequest<ServiceResponse<string>>;

    public class CommandHandler(
        INotificationRepository repository,
        IUserProvider userProvider) : IRequestHandler<Command, ServiceResponse<string>>
    {
        public async Task<ServiceResponse<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await userProvider.GetUserId(cancellationToken);

            var notification = await repository.MarkAsReadAsync(request.NotificationId, userId, cancellationToken);

            return notification is null
                ? ServiceResponse<string>.NotFound($"Notification {request.NotificationId} not found or it's system notification.")
                : ServiceResponse<string>.Ok();
        }
    }
}

