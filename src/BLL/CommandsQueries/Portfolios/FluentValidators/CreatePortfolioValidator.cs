using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Portfolio;
using FluentValidation;

namespace BLL.CommandsQueries.Portfolios.FluentValidators;

public class CreatePortfolioValidator : AbstractValidator<Create.Command<CreatePortfolioVM, PortfolioVM>>
{
    public CreatePortfolioValidator()
    {
        RuleFor(x => x.Model.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Model.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        // RuleFor(x => x.Model.PortfolioUrl)
        //     .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        //     .WithMessage("Portfolio URL must be a valid URL.");
    }
}