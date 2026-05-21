using Domain.Models.Notifications;

namespace BLL.ViewModels.Notification;

public class NotificationVM
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? UserId { get; set; }
    public string? LinkAddress { get; set; }
}