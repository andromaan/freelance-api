using Domain.Models.Notifications;

namespace BLL.ViewModels.Notification;

public enum NotificationTypeFreelancer
{
    NewMessage = NotificationType.NewMessage,
    InterestedInYourBid = NotificationType.InterestedInYourBid,    
    NotInterestedInYourBid = NotificationType.NotInterestedInYourBid, 
    MilestoneStatusUpdated = NotificationType.MilestoneStatusUpdated,
    // MilestoneApproved = NotificationType.MilestoneApproved,
    // MilestoneRejected = NotificationType.MilestoneRejected,
    ContractCreated = NotificationType.ContractCreated,
    PaymentReceived = NotificationType.PaymentReceived,
    DisputeOpened = NotificationType.DisputeOpened,
    ReviewLeft = NotificationType.ReviewLeft,
    SystemAnnouncement = NotificationType.SystemAnnouncement,
    ProjectDeadlineReminder = NotificationType.ProjectDeadlineReminder
}