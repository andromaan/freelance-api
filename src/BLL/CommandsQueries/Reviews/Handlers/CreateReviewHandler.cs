using System.Net;
using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Freelancers;
using BLL.Common.Interfaces.Repositories.Reviews;
using BLL.Common.Interfaces.Repositories.Users;
using BLL.Services;
using BLL.ViewModels.Reviews;
using Domain.Models.Contracts;
using Domain.Models.Reviews;

namespace BLL.CommandsQueries.Reviews.Handlers;

public class CreateReviewHandler(
    IUserProvider userProvider,
    IUserQueries userQueries,
    IContractQueries contractQueries,
    IFreelancerQueries freelancerQueries,
    IReviewQueries reviewQueries)
    : ICreateHandler<Review, CreateReviewVM, ReviewVM>
{
    public async Task<Result<ReviewVM?>> HandleAsync(Review entity, CreateReviewVM createModel,
        CancellationToken cancellationToken)
    {
        var contract = await contractQueries.GetByIdAsync(createModel.ContractId, cancellationToken);
        if (contract is null)
        {
            return Result<ReviewVM?>.NotFound($"Contract with Id {createModel.ContractId} not found");
        }

        if (contract.Status != ContractStatus.Completed)
        {
            return Result<ReviewVM?>.GetResponse(
                "You can only review a contract that has been completed",
                false, null, HttpStatusCode.BadRequest);
        }

        var reviewerId = await userProvider.GetUserId();

        var reviewerRole = (await userQueries.GetByIdAsync(reviewerId, cancellationToken))!.RoleId;

        var freelancer = await freelancerQueries.GetByUserIdAsync(reviewerId, cancellationToken);

        if (contract.CreatedBy != reviewerId && contract.FreelancerId != freelancer?.Id)
        {
            return Result<ReviewVM?>.Unauthorized("You are not authorized to review this contract");
        }

        // Determine the reviewed user ID
        if (contract.CreatedBy == reviewerId)
        {
            // Employer is reviewing the freelancer, so get freelancer's user ID
            var contractFreelancer = await freelancerQueries.GetByIdAsync(contract.FreelancerId, cancellationToken);
            if (contractFreelancer is null)
            {
                return Result<ReviewVM?>.NotFound("Freelancer not found");
            }

            entity.ReviewedUserId = contractFreelancer.CreatedBy;
        }
        else
        {
            // Freelancer is reviewing the employer
            entity.ReviewedUserId = contract.CreatedBy;
        }

        if (await reviewQueries.GetByReviewerAndReviewedUser(reviewerId, entity.ReviewedUserId, contract.Id, cancellationToken) is
            not null)
        {
            return Result<ReviewVM?>.GetResponse(
                "You have already reviewed this user",
                false, null, HttpStatusCode.BadRequest);
        }

        entity.ReviewerRoleId = reviewerRole;
        entity.ContractId = createModel.ContractId;

        return Result<ReviewVM?>.Ok();
    }
}