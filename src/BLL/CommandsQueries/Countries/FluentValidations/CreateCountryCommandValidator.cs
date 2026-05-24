using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Country;
using FluentValidation;

namespace BLL.CommandsQueries.Countries.FluentValidations;

public class CreateCountryCommandValidator : AbstractValidator<Create.Command<CreateCountryVM, CountryVM>>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Model.Name)
            .NotEmpty().WithMessage("Country name is required")
            .Must(name => name.Trim().Length > 0).WithMessage("Country name cannot be empty or whitespace")
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters");
        
        RuleFor(x => x.Model.Alpha2Code)
            .NotEmpty().WithMessage("Alpha-2 code is required")
            .Length(2).WithMessage("Alpha-2 code must be exactly 2 characters long");
        
        RuleFor(x => x.Model.Alpha3Code)
            .NotEmpty().WithMessage("Alpha-3 code is required")
            .Length(3).WithMessage("Alpha-3 code must be exactly 3 characters long");
    }
}