using BLL.Common.Handlers;
using BLL.Services;
using BLL.ViewModels;
using BLL.ViewModels.Notification;
using Domain.Models.Notifications;

namespace BLL.CommandsQueries.Notifications.Handlers;

public class GetAllFilteredNotificationsHandler
    : IGetAllFilteredHandler<Notification, FilterNotificationVM, NotificationVM>
{
    public Task<(Result<PaginatedItemsVM<NotificationVM>?> response, List<Notification>? filteredEntities)>
        HandleAsync(
            List<Notification> entities, FilterNotificationVM filter,
            CancellationToken cancellationToken)
    {
        List<Notification> filteredEntities = entities;

        if (filter.IsRead != null)
        {
            filteredEntities = filteredEntities.Where(e => e.IsRead == filter.IsRead).ToList();
        }

        if (filter.NotificationType != null)
        {
            filteredEntities = filteredEntities.Where(e => e.Type == filter.NotificationType).ToList();
        }

        return Task.FromResult<(Result<PaginatedItemsVM<NotificationVM>?>, List<Notification>?)>((
            Result<PaginatedItemsVM<NotificationVM>?>.Ok(), filteredEntities));
    }
}