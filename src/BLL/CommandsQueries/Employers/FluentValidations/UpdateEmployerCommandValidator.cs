using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Employer;
using FluentValidation;

namespace BLL.CommandsQueries.Employers.FluentValidations;

public class UpdateEmployerCommandValidator : AbstractValidator<UpdateByUser.Command<UpdateEmployerVM, EmployerVM>>
{
    public UpdateEmployerCommandValidator()
    {
        RuleFor(x => x.Model.CompanyName)
            .NotEmpty().WithMessage("Company name is required");
        
        RuleFor(x => x.Model.CompanyWebsite)
            .NotEmpty().WithMessage("Company website is required");
    }
}