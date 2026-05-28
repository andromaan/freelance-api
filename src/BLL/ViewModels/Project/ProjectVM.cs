using BLL.ViewModels.Category;

namespace BLL.ViewModels.Project;

public class ProjectVM
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Budget { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public List<CategoryVM> Categories { get; set; } = new();
    public Guid CreatedBy { get; set; }
}