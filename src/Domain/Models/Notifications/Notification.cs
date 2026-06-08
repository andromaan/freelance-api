using Domain.Common.Abstractions;

namespace Domain.Models.Notifications;

public class Notification : Entity<Guid>
{
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? UserId { get; set; }
    public string? LinkAddress { get; set; }
}

public enum NotificationType
{
    NewBidReceived,         // Новий bid на проект (для роботодавця)
    InterestedInYourBid,    // Ставкою зацікавилися
    NotInterestedInYourBid, // Ставкою не зацікавилися
    NewMessage,             // Нове повідомлення в чаті
    MilestoneStatusUpdated,
    // MilestoneApproved,      // Milestone схвалено (для фрілансера)
    // MilestoneRejected,      // Milestone відхилено (для фрілансера)
    ContractCreated,        // Новий контракт створено (для обох сторін)
    PaymentReceived,        // Оплата надійшла (для фрілансера)
    DisputeOpened,          // Відкрито спір (для обох сторін)
    ReviewLeft,             // Залишено відгук (для фрілансера/роботодавця)
    SystemAnnouncement,     // Глобальна розсилка (наприклад, "Оновлення платформи")
    ProjectDeadlineReminder, // Нагадування про дедлайн проекту
    ProposalAccepted,         // Пропозиція прийнята (для фрілансера)
    ProposalRejected,         // Пропозиція відхилена (для фрілансера)
    NewQuoteReceived,      // Новий quote на проект (для роботодавця)
}