using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Freelancer;
using FluentValidation;

namespace BLL.CommandsQueries.Freelancers.FluentValidations;

public class UpdateFreelancerCommandValidator : AbstractValidator<UpdateByUser.Command<UpdateFreelancerVM, FreelancerVM>>
{
    public UpdateFreelancerCommandValidator()
    {
        RuleFor(x => x.Model.Bio)
            .NotEmpty().WithMessage("Bio is required")
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters");

        RuleFor(x => x.Model.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");
    }
}