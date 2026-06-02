using Domain.Common.Abstractions;
using Domain.Models.Projects;

namespace Domain.Models.Freelance;

public class Freelancer : AuditableEntity<Guid>
{
    public string? Bio { get; set; }
    public string? Location { get; set; }
    
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Portfolio> Portfolio { get; set; } = new List<Portfolio>();
}