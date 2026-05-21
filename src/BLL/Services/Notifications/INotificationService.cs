using Domain.Models.Notifications;

namespace BLL.Services.Notifications;

public interface INotificationService
{
    /// <summary>
    /// Зберігає нотифікацію в БД та надсилає її через SignalR конкретному користувачу (або всім, якщо userId == null).
    /// </summary>
    Task SendAsync(
        string message,
        NotificationType type,
        Guid? userId,
        CancellationToken cancellationToken = default,
        string? linkAddress = null);
}