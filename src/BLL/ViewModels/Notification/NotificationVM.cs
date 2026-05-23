namespace BLL.ViewModels.Notification;

public class NotificationVM
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? UserId { get; set; }
    public string? LinkAddress { get; set; }
}