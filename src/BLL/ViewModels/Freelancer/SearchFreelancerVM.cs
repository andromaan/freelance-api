using BLL.ViewModels.Country;
using BLL.ViewModels.Skill;
using BLL.ViewModels.UserLanguage;

namespace BLL.ViewModels.Freelancer;

public class SearchFreelancerVM
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? DisplayName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? AvatarImg { get; set; }
    
    public string? Bio { get; set; }
    public string? Location { get; set; }
    
    public List<SkillVM> Skills { get; set; } = new();
    public List<UserLanguageVM> Languages { get; set; } = new();
    
    public CountryVM? Country { get; set; }
    
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
}
