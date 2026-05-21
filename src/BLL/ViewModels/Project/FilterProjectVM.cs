namespace BLL.ViewModels.Project;

public class FilterProjectVM
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public DateTime? DeadlineMax { get; set; }
    public List<int>? CategoryIds { get; set; } = new();
}