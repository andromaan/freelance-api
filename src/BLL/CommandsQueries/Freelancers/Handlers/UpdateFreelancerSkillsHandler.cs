using BLL.Common.Handlers;
using BLL.Common.Interfaces.Repositories.Skills;
using BLL.Services;
using BLL.ViewModels.Freelancer;
using Domain.Models.Freelance;

namespace BLL.CommandsQueries.Freelancers.Handlers;

public class UpdateFreelancerSkillsHandler(ISkillQueries skillQueries)
    : IUpdateHandler<Freelancer, UpdateFreelancerSkillsVM, FreelancerVM>
{
    public async Task<Result<FreelancerVM?>> HandleAsync(Freelancer existingEntity,
        UpdateFreelancerSkillsVM updateModel,
        CancellationToken cancellationToken)
    {
        existingEntity.Skills.Clear();

        foreach (var langId in updateModel.SkillIds.Distinct())
        {
            var existingSkill = await skillQueries.GetByIdAsync(langId, cancellationToken);
            if (existingSkill == null)
            {
                return Result<FreelancerVM?>.NotFound($"Skill with id {langId} not found");
            }

            existingEntity.Skills.Add(existingSkill);
        }

        return Result<FreelancerVM?>.Ok();
    }
}