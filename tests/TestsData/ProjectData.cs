using Domain.Models.Projects;

namespace TestsData;

public class ProjectData
{
    public static Project CreateProject(Guid? id = null, Guid? userId = null, decimal? budget = null)
    {
        return new Project
        {
            Id = id ?? Guid.NewGuid(),
            Title = "Test Project",
            Description = "Test Project Description",
            Budget = budget ?? 5000m,
            Status = ProjectStatus.Open,
            CreatedBy = userId ?? Guid.NewGuid(),
            Deadline = DateTime.UtcNow.AddMonths(1)
        };
    }
}