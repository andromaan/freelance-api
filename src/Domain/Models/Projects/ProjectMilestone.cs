using Domain.Common.Abstractions;

namespace Domain.Models.Projects;

public class ProjectMilestone : AuditableEntity<Guid>
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}