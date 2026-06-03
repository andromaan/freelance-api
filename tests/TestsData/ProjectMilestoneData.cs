using Domain.Models.Projects;

namespace TestsData;

public class ProjectMilestoneData
{
    public static ProjectMilestone CreateProjectMilestone(Guid projectId, Guid? userId = null, decimal amount = 0)
    {
        return new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Description = "Test Project Milestone",
            Amount = amount,
            DueDate = DateTime.UtcNow.AddDays(45),
            CreatedBy = userId ?? Guid.NewGuid()
        };
    }
}