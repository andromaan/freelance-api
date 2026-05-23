using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.ContractMilestone;
using FluentValidation;

namespace BLL.CommandsQueries.ContractMilestones.FluentValidations;

public class
    UpdCMilestoneForEmployerCmdValidation : AbstractValidator<Update.Command<UpdContractMilestoneStatusEmployerVM, Guid, ContractMilestoneVM>>
{
    public UpdCMilestoneForEmployerCmdValidation()
    {
        RuleFor(x => x.Model.Status).IsInEnum();
    }
}