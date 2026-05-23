using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Contract;
using FluentValidation;

namespace BLL.CommandsQueries.Contracts.FluentValidations;

public class UpdateContractCommandValidator : AbstractValidator<Update.Command<UpdateContractVM, Guid, ContractVM>>
{
    public UpdateContractCommandValidator()
    {
        RuleFor(x => x.Model.EndDate)
            .GreaterThan(x => x.Model.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Model.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.Model.EndDate)
            .NotEmpty().WithMessage("End date is required");
    }
}
