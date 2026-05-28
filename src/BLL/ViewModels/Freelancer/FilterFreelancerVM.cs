namespace BLL.ViewModels.Freelancer;

public class FilterFreelancerVM
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<int>? SkillIds { get; set; } = new();
    public decimal? MinRating { get; set; }
    public List<int>? LanguageIds { get; set; } = new();
    public List<int>? CountryIds { get; set; } = new();
}
