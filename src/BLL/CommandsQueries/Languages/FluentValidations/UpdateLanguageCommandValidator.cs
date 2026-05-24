using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Language;
using FluentValidation;

namespace BLL.CommandsQueries.Languages.FluentValidations;

public class UpdateLanguageCommandValidator : AbstractValidator<Update.Command<CreateLanguageVM, int, LanguageVM>>
{
    public UpdateLanguageCommandValidator()
    {
        RuleFor(x => x.Model.Name)
            .NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Model.Code)
            .NotEmpty().WithMessage("Code is required.");
    }
}
