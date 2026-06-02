using Domain.Common.Abstractions;
using Domain.Models.Contracts;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class Message : AuditableEntity<Guid>
{
    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public required Guid ReceiverId { get; set; }
    public User? Receiver { get; set; }

    public required string Text { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public bool IsEdited { get; set; } = false;
}