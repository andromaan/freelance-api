using BLL.Common.Interfaces;

namespace BLL.ViewModels.ProjectMilestone;

public class UpdateProjectMilestoneVM : ISkipMapper
{
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}