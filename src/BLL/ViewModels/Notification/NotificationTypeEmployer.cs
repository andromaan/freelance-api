using Domain.Models.Notifications;

namespace BLL.ViewModels.Notification;

public enum NotificationTypeEmployer
{
    NewBidReceived = NotificationType.NewBidReceived,
    NewMessage = NotificationType.NewMessage,
    DisputeOpened = NotificationType.DisputeOpened,
    ReviewLeft = NotificationType.ReviewLeft,
    SystemAnnouncement = NotificationType.SystemAnnouncement,
    ProjectDeadlineReminder = NotificationType.ProjectDeadlineReminder,
    NewQuoteReceived = NotificationType.NewQuoteReceived,
}