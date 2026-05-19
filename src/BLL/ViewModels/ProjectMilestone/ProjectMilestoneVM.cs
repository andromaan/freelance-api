namespace BLL.ViewModels.ProjectMilestone;

public class ProjectMilestoneVM
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}