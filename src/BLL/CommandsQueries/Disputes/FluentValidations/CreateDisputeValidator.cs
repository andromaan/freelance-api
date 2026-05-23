using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.Dispute;
using FluentValidation;

namespace BLL.CommandsQueries.Disputes.FluentValidations;

public class CreateDisputeValidator : AbstractValidator<Create.Command<CreateDisputeVM, DisputeVM>>
{
    public CreateDisputeValidator()
    {
        RuleFor(d => d.Model.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(100).WithMessage("Reason cannot exceed 100 characters.");

        RuleFor(d => d.Model.ContractId)
            .NotEmpty().WithMessage("Contract Id is required.");
    }
}