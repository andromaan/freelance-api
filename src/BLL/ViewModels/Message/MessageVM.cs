namespace BLL.ViewModels.Message;

public class MessageVM
{
    public Guid Id { get; set; }
    public Guid? ContractId { get; set; }

    public Guid ReceiverId { get; set; }
    public Guid SenderId { get; set; }

    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsEdited { get; set; }
}