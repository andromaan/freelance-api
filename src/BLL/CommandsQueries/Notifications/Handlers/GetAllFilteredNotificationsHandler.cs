using BLL.Common.Handlers;
using BLL.Services;
using BLL.ViewModels.Notification;
using Domain.Models.Notifications;

namespace BLL.CommandsQueries.Notifications.Handlers;

public class GetAllFilteredNotificationsHandler
    : IGetAllFilteredHandler<Notification, FilterNotificationVM>
{
    public Task<(ServiceResponse response, List<Notification>? filteredEntities)> HandleAsync(
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

        return Task.FromResult<(ServiceResponse, List<Notification>?)>((ServiceResponse.Ok(), filteredEntities));
    }
}