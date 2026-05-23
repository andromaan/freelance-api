using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Notifications;


public class MarkAllNotificationsAsRead
{
    public record Command : IRequest<ServiceResponse<string>>;

    public class CommandHandler(
        INotificationRepository repository,
        IUserProvider userProvider) : IRequestHandler<Command, ServiceResponse<string>>
    {
        public async Task<ServiceResponse<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await userProvider.GetUserId(cancellationToken);

            var updatedCount = await repository.MarkAllAsReadAsync(userId, cancellationToken);

            return ServiceResponse<string>.Ok($"{updatedCount} notification(s) marked as read.");
        }
    }
}

