using Domain.Common.Abstractions;
using Domain.Models.Freelance;

namespace Domain.Models.Projects;

public class Bid : AuditableEntity<Guid>
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid FreelancerId { get; set; }
    public Freelancer? Freelancer { get; set; }
    
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    
    public bool? IsInteresting { get; set; }
}