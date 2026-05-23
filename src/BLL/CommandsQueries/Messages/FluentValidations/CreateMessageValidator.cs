using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Message;
using FluentValidation;

namespace BLL.CommandsQueries.Messages.FluentValidations;

public class CreateMessageValidator : AbstractValidator<Create.Command<CreateMessageVM, MessageVM>>
{
    public CreateMessageValidator()
    {
        RuleFor(m => m.Model.Text)
            .NotEmpty().WithMessage("Text cannot be empty")
            .MaximumLength(2000).WithMessage("Text cannot exceed 2000 characters");

        RuleFor(m => m.Model.ContractId)
            .NotEmpty().WithMessage("Contract ID is required");

        RuleFor(m => m.Model.ReceiverEmail)
            .NotEmpty().WithMessage("Receiver email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
