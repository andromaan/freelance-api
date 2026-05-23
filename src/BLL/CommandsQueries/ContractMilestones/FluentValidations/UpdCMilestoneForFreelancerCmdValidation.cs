using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.ContractMilestone;
using FluentValidation;

namespace BLL.CommandsQueries.ContractMilestones.FluentValidations;

public class
    UpdCMilestoneForFreelancerCmdValidation : AbstractValidator<Update.Command<UpdContractMilestoneStatusFreelancerVM, Guid, ContractMilestoneVM>>
{
    public UpdCMilestoneForFreelancerCmdValidation()
    {
        RuleFor(x => x.Model.Status).IsInEnum();
    }
}