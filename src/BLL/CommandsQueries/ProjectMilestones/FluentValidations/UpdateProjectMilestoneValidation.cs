using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.ProjectMilestone;
using FluentValidation;

namespace BLL.CommandsQueries.ProjectMilestones.FluentValidations;

public class UpdateProjectMilestoneValidation
    : AbstractValidator<Update.Command<UpdateProjectMilestoneVM, Guid, ProjectMilestoneVM>>
{
    public UpdateProjectMilestoneValidation()
    {
        RuleFor(m => m.Model.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future");
    }
}