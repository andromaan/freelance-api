using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.ViewModels.Reviews;
using FluentValidation;

namespace BLL.CommandsQueries.Reviews.FluentValidations;

public class UpdateReviewValidation : AbstractValidator<Update.Command<UpdateReviewVM, Guid, ReviewVM>>
{
    public UpdateReviewValidation()
    {
        RuleFor(r=> r.Model.ReviewText)
            .NotEmpty()
            .WithMessage("The review text cannot be empty");
        
        RuleFor(r=> r.Model.Rating)
            .InclusiveBetween(0.0m, 5.0m)
            .WithMessage("The rating must be between 0.0 and 5.0.");
    }
}