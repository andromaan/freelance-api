using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.ProjectMilestone;
using FluentValidation;

namespace BLL.CommandsQueries.ProjectMilestones.FluentValidations;

public class CreateProjectMilestoneValidation 
    : AbstractValidator<Create.Command<CreateProjectMilestoneVM, ProjectMilestoneVM>>
{
    public CreateProjectMilestoneValidation()
    {
        RuleFor(m => m.Model.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future");
    }
}