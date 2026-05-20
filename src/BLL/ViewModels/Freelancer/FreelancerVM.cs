using BLL.ViewModels.Portfolio;
using BLL.ViewModels.Skill;

namespace BLL.ViewModels.Freelancer;

public class FreelancerVM
{
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public List<SkillVM> Skills { get; set; } = [];
    public List<PortfolioVM> Portfolio { get; set; } = [];
    public Guid CreatedBy { get; set; }
}