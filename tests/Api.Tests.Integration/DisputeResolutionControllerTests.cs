using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.DisputeResolution;
using DAL.Extensions;
using Domain.Models.Contracts;
using Domain.Models.Disputes;
using Domain.Models.Freelance;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class DisputeResolutionControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.AdminRole), IAsyncLifetime
{
    private User _adminUser = null!;
    private User _employerUser = null!;
    private User _freelancerUser = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private Contract _contract = null!;
    private Dispute _dispute = null!;
    private DisputeResolution _existingResolution = null!;

    [Fact]
    public async Task ShouldCreateDisputeResolution()
    {
        // Arrange
        var newDispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "New dispute to resolve",
            status: DisputeStatus.UnderReview,
            createdById: _employerUser.Id
        );
        await Context.AddAuditableAsync(newDispute);
        await SaveChangesAsync();

        var request = new CreateDisputeResolutionVM
        {
            DisputeId = newDispute.Id,
            ResolutionDetails = "Dispute resolved in favor of the employer. Freelancer will complete remaining work.",
            DisputeStatus = DisputeResolutionStatusForModerator.Resolved
        };

        // Act
        var response = await Client.PostAsJsonAsync("DisputeResolution", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var resolutionFromResponse = await JsonHelper.GetPayloadAsync<DisputeResolutionVM>(response);
        var resolutionId = resolutionFromResponse.Id;

        var resolutionFromDb = await Context.DisputeResolutions.FirstOrDefaultAsync(x => x.Id == resolutionId);

        resolutionFromDb.Should().NotBeNull();
        resolutionFromDb.DisputeId.Should().Be(newDispute.Id);
        resolutionFromDb.ResolutionDetails.Should().Be(request.ResolutionDetails);
        resolutionFromDb.CreatedBy.Should().Be(UserId);

        // Verify dispute status was updated
        var updatedDispute = await Context.Disputes.FirstOrDefaultAsync(x => x.Id == newDispute.Id);
        updatedDispute.Should().NotBeNull();
        updatedDispute.Status.Should().Be(DisputeStatus.Resolved);
    }

    [Fact]
    public async Task ShouldGetAllDisputeResolutions()
    {
        // Act
        var response = await Client.GetAsync("DisputeResolution");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var resolutionsFromResponse = await JsonHelper.GetPayloadAsync<List<DisputeResolutionVM>>(response);
        
        resolutionsFromResponse.Should().NotBeNull();
        resolutionsFromResponse.Should().HaveCountGreaterThan(0);
        resolutionsFromResponse.Should().Contain(r => r.Id == _existingResolution.Id);
    }

    [Fact]
    public async Task ShouldGetDisputeResolutionById()
    {
        // Act
        var response = await Client.GetAsync($"DisputeResolution/{_existingResolution.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var resolutionFromResponse = await JsonHelper.GetPayloadAsync<DisputeResolutionVM>(response);
        
        resolutionFromResponse.Id.Should().Be(_existingResolution.Id);
        resolutionFromResponse.DisputeId.Should().Be(_existingResolution.DisputeId);
        resolutionFromResponse.ResolutionDetails.Should().Be(_existingResolution.ResolutionDetails);
        resolutionFromResponse.ResolvedById.Should().Be(_existingResolution.CreatedBy);
    }

    [Fact]
    public async Task ShouldDeleteDisputeResolution()
    {
        // Arrange
        var newDispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "Dispute for resolution to delete",
            status: DisputeStatus.Resolved,
            createdById: _employerUser.Id
        );
        var resolutionToDelete = DisputeResolutionData.CreateDisputeResolution(
            disputeId: newDispute.Id,
            resolutionDetails: "Resolution to delete",
            createdById: UserId
        );
        
        await Context.AddAuditableAsync(newDispute);
        await Context.AddAuditableAsync(resolutionToDelete);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"DisputeResolution/{resolutionToDelete.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var resolutionFromDb = await Context.DisputeResolutions.FirstOrDefaultAsync(x => x.Id == resolutionToDelete.Id);
        resolutionFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenGettingNonExistentResolution()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"DisputeResolution/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenDeletingNonExistentResolution()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"DisputeResolution/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenCreatingResolutionForNonExistentDispute()
    {
        // Arrange
        var request = new CreateDisputeResolutionVM
        {
            DisputeId = Guid.NewGuid(),
            ResolutionDetails = "Resolution for non-existent dispute",
            DisputeStatus = DisputeResolutionStatusForModerator.Resolved
        };

        // Act
        var response = await Client.PostAsJsonAsync("DisputeResolution", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenResolvingAlreadyResolvedDispute()
    {
        // Arrange
        var resolvedDispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "Already resolved dispute",
            status: DisputeStatus.Resolved,
            createdById: _employerUser.Id
        );
        await Context.AddAuditableAsync(resolvedDispute);
        await SaveChangesAsync();

        var request = new CreateDisputeResolutionVM
        {
            DisputeId = resolvedDispute.Id,
            ResolutionDetails = "Trying to resolve already resolved dispute",
            DisputeStatus = DisputeResolutionStatusForModerator.Resolved
        };

        // Act
        var response = await Client.PostAsJsonAsync("DisputeResolution", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenResolvingRejectedDispute()
    {
        // Arrange
        var rejectedDispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "Rejected dispute",
            status: DisputeStatus.Rejected,
            createdById: _employerUser.Id
        );
        await Context.AddAuditableAsync(rejectedDispute);
        await SaveChangesAsync();

        var request = new CreateDisputeResolutionVM
        {
            DisputeId = rejectedDispute.Id,
            ResolutionDetails = "Trying to resolve rejected dispute",
            DisputeStatus = DisputeResolutionStatusForModerator.Resolved
        };

        // Act
        var response = await Client.PostAsJsonAsync("DisputeResolution", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldRejectDisputeWhenResolutionStatusIsRejected()
    {
        // Arrange
        var newDispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "Dispute to reject",
            status: DisputeStatus.UnderReview,
            createdById: _employerUser.Id
        );
        await Context.AddAuditableAsync(newDispute);
        await SaveChangesAsync();

        var request = new CreateDisputeResolutionVM
        {
            DisputeId = newDispute.Id,
            ResolutionDetails = "Dispute rejected - insufficient evidence provided",
            DisputeStatus = DisputeResolutionStatusForModerator.Rejected
        };

        // Act
        var response = await Client.PostAsJsonAsync("DisputeResolution", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verify dispute status was updated to Rejected
        var updatedDispute = await Context.Disputes.FirstOrDefaultAsync(x => x.Id == newDispute.Id);
        updatedDispute.Should().NotBeNull();
        updatedDispute.Status.Should().Be(DisputeStatus.Rejected);
    }

    public async Task InitializeAsync()
    {
        _adminUser = UserData.CreateTestUser(
            id: UserId,
            email: "admin@test.com",
            roleId: GetRoleIdByName(Settings.Roles.AdminRole)
        );

        _employerUser = UserData.CreateTestUser(
            email: "employer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.EmployerRole)
        );

        _freelancerUser = UserData.CreateTestUser(
            email: "freelancer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.FreelancerRole)
        );

        _project = ProjectData.CreateProject(userId: _employerUser.Id);
        _freelancer = FreelancerData.CreateFreelancer(userId: _freelancerUser.Id);

        _contract = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            createdById: _employerUser.Id
        );

        _dispute = DisputeData.CreateDispute(
            contractId: _contract.Id,
            reason: "Test dispute for resolution testing",
            status: DisputeStatus.Resolved,
            createdById: _employerUser.Id
        );

        _existingResolution = DisputeResolutionData.CreateDisputeResolution(
            disputeId: _dispute.Id,
            resolutionDetails: "Original resolution for testing - dispute resolved",
            createdById: _adminUser.Id
        );

        await Context.AddAuditableAsync(_adminUser);
        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_freelancerUser);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_contract);
        await Context.AddAuditableAsync(_dispute);
        await Context.AddAuditableAsync(_existingResolution);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}
