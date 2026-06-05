namespace BLL.ViewModels.User;

public class FilterUserVM
{
    public string? Search { get; set; }
    public List<int>? RoleIds { get; set; } = new();
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
