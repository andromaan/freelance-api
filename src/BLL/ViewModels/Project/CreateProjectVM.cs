namespace BLL.ViewModels.Project;

public class CreateProjectVM
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Budget { get; set; }
    public DateTime Deadline { get; set; }

    public List<int> CategoryIds { get; set; } = new();
}