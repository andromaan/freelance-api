using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Services;
using MediatR;

namespace BLL.CommandsQueries.Notifications;


public class MarkAllNotificationsAsRead
{
    public record Command : IRequest<Result<string>>;

    public class CommandHandler(
        INotificationRepository repository,
        IUserProvider userProvider) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await userProvider.GetUserId(cancellationToken);

            var updatedCount = await repository.MarkAllAsReadAsync(userId, cancellationToken);

            return Result<string>.Ok($"{updatedCount} notification(s) marked as read.");
        }
    }
}

