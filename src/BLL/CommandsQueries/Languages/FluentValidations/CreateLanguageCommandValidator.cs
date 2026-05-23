using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Language;
using FluentValidation;

namespace BLL.CommandsQueries.Languages.FluentValidations;

public class CreateLanguageCommandValidator : AbstractValidator<Create.Command<CreateLanguageVM, LanguageVM>>
{
    public CreateLanguageCommandValidator()
    {
        RuleFor(x => x.Model.Name)
            .NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Model.Code)
            .NotEmpty().WithMessage("Code is required.");
    }
}

