using AutoMapper;
using BLL.Common.Interfaces.Repositories.Notifications;
using BLL.Hubs;
using BLL.ViewModels.Notification;
using Domain.Models.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace BLL.Services.Notifications;

public class NotificationService(
    INotificationRepository notificationRepository,
    IHubContext<NotificationHub> hubContext,
    IMapper mapper)
    : INotificationService
{
    public async Task SendAsync(
        string message,
        NotificationType type,
        Guid? userId,
        CancellationToken cancellationToken = default,
        string? linkAddress = null)
    {
        // 1. Зберігаємо в БД
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Message = message,
            Type = type,
            IsRead = false,
            SentAt = DateTime.UtcNow,
            UserId = userId,
            LinkAddress = linkAddress
        };

        await notificationRepository.CreateAsync(notification, cancellationToken);

        // 2. Надсилаємо через SignalR
        if (userId is null)
        {
            await hubContext.Clients.All
                .SendAsync("ReceiveNotification", mapper.Map<NotificationVM>(notification), cancellationToken);
        }
        else
        {
            await hubContext.Clients.User(userId.ToString()!)
                .SendAsync("ReceiveNotification", mapper.Map<NotificationVM>(notification), cancellationToken);
        }
    }
}