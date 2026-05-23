using BLL.Common.Handlers;
using BLL.Common.Interfaces;
using BLL.Common.Interfaces.Repositories.Contracts;
using BLL.Common.Interfaces.Repositories.Disputes;
using BLL.Services;
using BLL.ViewModels.DisputeResolution;
using Domain.Models.Contracts;
using Domain.Models.Disputes;

namespace BLL.CommandsQueries.DisputeResolutions.Handlers;

public class CreateDisputeResolutionHandler(
    IUserProvider userProvider,
    IDisputeQueries disputeQueries,
    IDisputeRepository disputeRepository,
    IContractQueries contractQueries,
    IContractRepository contractRepository
) : ICreateHandler<DisputeResolution, CreateDisputeResolutionVM, DisputeResolutionVM>
{
    public async Task<ServiceResponse<DisputeResolutionVM?>> HandleAsync(DisputeResolution entity, CreateDisputeResolutionVM createModel,
        CancellationToken cancellationToken)
    {
        // 1. Validate only moderators/admins can create resolutions
        var userRole = userProvider.GetUserRole();
        var isAdminOrModerator = userRole == Settings.Roles.AdminRole || userRole == Settings.Roles.ModeratorRole;
        
        if (!isAdminOrModerator)
        {
            return ServiceResponse<DisputeResolutionVM?>.Unauthorized("Only moderators and administrators can resolve disputes");
        }

        // 2. Check if dispute exists
        var existingDispute = await disputeQueries.GetByIdAsync(createModel.DisputeId, cancellationToken);
        
        if (existingDispute is null)
        {
            return ServiceResponse<DisputeResolutionVM?>.NotFound($"Dispute with Id {createModel.DisputeId} not found");
        }

        // 3. Check dispute status - can only resolve Open or UnderReview disputes
        if (existingDispute.Status == DisputeStatus.Resolved)
        {
            return ServiceResponse<DisputeResolutionVM?>.BadRequest("This dispute has already been resolved");
        }
        
        if (existingDispute.Status == DisputeStatus.Rejected)
        {
            return ServiceResponse<DisputeResolutionVM?>.BadRequest("This dispute has already been rejected");
        }

        // 4. Get the associated contract
        var contract = await contractQueries.GetByIdAsync(existingDispute.ContractId, cancellationToken);
        
        if (contract is null)
        {
            return ServiceResponse<DisputeResolutionVM?>.NotFound($"Contract with Id {existingDispute.ContractId} not found");
        }

        // 5. Update dispute status based on moderator's decision
        existingDispute.Status = (DisputeStatus)createModel.DisputeStatus;

        // 6. Update contract status based on resolution
        // If dispute is resolved favorably, contract can continue (Active or mark as resolved)
        // If dispute is rejected, return contract to its previous state before dispute
        if (createModel.DisputeStatus == DisputeResolutionStatusForModerator.Resolved)
        {
            // Dispute was valid and resolved - keep contract in Disputed state or move to specific resolution state
            // The actual financial transactions and milestone updates should be handled separately
            // For now, we keep the contract as Disputed until manual intervention or separate process
            contract.Status = ContractStatus.Disputed; // This could be changed to Resolved or Refunded based on resolution details
        }
        else if (createModel.DisputeStatus == DisputeResolutionStatusForModerator.Rejected)
        {
            // Dispute was invalid - return contract to Active state
            if (contract.Status == ContractStatus.Disputed)
            {
                contract.Status = ContractStatus.Active;
            }
        }

        // 7. Save updates to database
        try
        {
            await disputeRepository.UpdateAsync(existingDispute, cancellationToken);
            await contractRepository.UpdateAsync(contract, cancellationToken);
        }
        catch (Exception e)
        {
            return ServiceResponse<DisputeResolutionVM?>.InternalError(e.Message);
        }

        return ServiceResponse<DisputeResolutionVM?>.Ok();
    }
}