using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.ViewModels.DisputeResolution;
using FluentValidation;

namespace BLL.CommandsQueries.DisputeResolutions.FluentValidations;

public class CreateDisputeResolutionValidation : AbstractValidator<Create.Command<CreateDisputeResolutionVM, DisputeResolutionVM>>
{
    public CreateDisputeResolutionValidation()
    {
        RuleFor(x => x.Model.DisputeId)
            .NotEmpty().WithMessage("Dispute ID is required.");

        RuleFor(x => x.Model.ResolutionDetails)
            .NotEmpty().WithMessage("Resolution details are required.")
            .MaximumLength(1000).WithMessage("Resolution details cannot exceed 1000 characters.");
    }
}