using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Message;
using FluentValidation;

namespace BLL.CommandsQueries.Messages.FluentValidations;

public class CreateMessageWithoutContractValidator : AbstractValidator<Create.Command<CreateMessageWithoutContractVM, MessageVM>>
{
    public CreateMessageWithoutContractValidator()
    {
        RuleFor(m => m.Model.Text)
            .NotEmpty().WithMessage("Text cannot be empty")
            .MaximumLength(2000).WithMessage("Text cannot exceed 2000 characters");

        RuleFor(m => m.Model.ReceiverEmail)
            .NotEmpty().WithMessage("Receiver email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}