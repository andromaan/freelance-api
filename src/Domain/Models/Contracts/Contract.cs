using Domain.Common.Abstractions;
using Domain.Models.Freelance;
using Domain.Models.Projects;

namespace Domain.Models.Contracts;

public class Contract : AuditableEntity<Guid>
{
    public required Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public required Guid FreelancerId { get; set; }
    public Freelancer? Freelancer { get; set; }
    
    // Employer is in Project as CreatedBy
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal AgreedRate { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Pending;
}

public enum ContractStatus
{
    Pending,
    Active,
    Completed,
    Cancelled,
    Disputed,
    Refunded
}
