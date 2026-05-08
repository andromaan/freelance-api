using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.ContractMilestone;
using DAL.Extensions;
using Domain.Models.Contracts;
using Domain.Models.Freelance;
using Domain.Models.Payments;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration.ContractMilestones;

public class ContractMilestoneModeratorControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.ModeratorRole), IAsyncLifetime
{
    private ContractMilestone _contractMilestone = null!;
    private Contract _contract = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private User _freelancerUser = null!;
    private User _employerUser = null!;
    private User _moderatorUser = null!;

    [Fact]
    public async Task ShouldApproveContractMilestone_AndCreateWalletTransactionsAndContractPayment()
    {
        // Arrange
        var userEmployerWalletAmountBefore = 10000m;
        var employerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = userEmployerWalletAmountBefore,
            Currency = "USD",
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };
        var freelancerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 0m,
            Currency = "USD",
            CreatedBy = _freelancerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _freelancerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        var milestone = new ContractMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = _contract.Id,
            Description = "Milestone for approval",
            Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = ContractMilestoneStatus.Submitted,
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        await Context.AddAuditableAsync(employerWallet);
        await Context.AddAuditableAsync(freelancerWallet);
        await Context.AddAuditableAsync(milestone);
        await SaveChangesAsync();

        var request = new UpdContractMilestoneStatusModeratorVM
        {
            Status = ContractMilestoneStatus.Approved
        };

        // Act
        var response = await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone.Id}/moderator", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verify milestone status changed
        var updatedMilestone = await Context.ContractMilestones
            .FirstOrDefaultAsync(x => x.Id == milestone.Id);
        updatedMilestone.Should().NotBeNull();
        updatedMilestone.Status.Should().Be(ContractMilestoneStatus.Approved);

        // Verify employer wallet was debited
        var updatedEmployerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _employerUser.Id);
        updatedEmployerWallet.Should().NotBeNull();
        updatedEmployerWallet.Balance.Should().Be(userEmployerWalletAmountBefore - milestone.Amount); // 10000 - 500

        // Verify freelancer wallet was credited
        var updatedFreelancerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _freelancerUser.Id);
        updatedFreelancerWallet.Should().NotBeNull();
        updatedFreelancerWallet.Balance.Should().Be(milestone.Amount); // 0 + 500

        // Verify wallet transactions were created
        var employerTransaction = await Context.Set<WalletTransaction>()
            .FirstOrDefaultAsync(t => t.WalletId == updatedEmployerWallet.Id && t.Amount == -milestone.Amount);
        employerTransaction.Should().NotBeNull();
        employerTransaction.Amount.Should().Be(-milestone.Amount);

        var freelancerTransaction = await Context.Set<WalletTransaction>()
            .FirstOrDefaultAsync(t => t.WalletId == updatedFreelancerWallet.Id && t.Amount == milestone.Amount);
        freelancerTransaction.Should().NotBeNull();
        freelancerTransaction.Amount.Should().Be(milestone.Amount);

        // Verify contract payment was created
        var contractPayment = await Context.Set<ContractPayment>()
            .FirstOrDefaultAsync(p => p.ContractId == _contract.Id && p.Amount == milestone.Amount);
        contractPayment.Should().NotBeNull();
        contractPayment.Amount.Should().Be(milestone.Amount);
        contractPayment.MilestoneId.Should().Be(milestone.Id);
    }

    [Fact]
    public async Task ShouldUpdateContractStatusToCompleted_WhenAllMilestonesApprovedOrRejected()
    {
        // Arrange
        var contract =
            ContractData.CreateContract(projectId: _project.Id, freelancerId: _freelancer.Id, agreedRate: 1000m,
                createdById: _employerUser.Id);
        await Context.AddAuditableAsync(contract);
        var milestone1 = new ContractMilestone
        {
            Id = Guid.NewGuid(), ContractId = contract.Id, Description = "M1", Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(1), Status = ContractMilestoneStatus.Submitted, CreatedBy = _employerUser.Id
        };
        var milestone2 = new ContractMilestone
        {
            Id = Guid.NewGuid(), ContractId = contract.Id, Description = "M2", Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(2), Status = ContractMilestoneStatus.Submitted, CreatedBy = _employerUser.Id
        };
        var employerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 10000m,
            Currency = "USD",
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };
        var freelancerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 0m,
            Currency = "USD",
            CreatedBy = _freelancerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _freelancerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        await Context.AddAuditableAsync(milestone1);
        await Context.AddAuditableAsync(milestone2);
        await Context.AddAuditableAsync(employerWallet);
        await Context.AddAuditableAsync(freelancerWallet);
        await SaveChangesAsync();

        // Act: Approve both milestones
        var approveVm = new UpdContractMilestoneStatusModeratorVM { Status = ContractMilestoneStatus.Approved };
        await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone1.Id}/moderator", approveVm);
        await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone2.Id}/moderator", approveVm);

        // Assert
        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contract.Id);
        contractFromDb.Should().NotBeNull();
        contractFromDb.Status.Should().Be(ContractStatus.Completed);
        
        var projectFromDb = await Context.Set<Project>().FirstOrDefaultAsync(x => x.Id == _project.Id);
        projectFromDb.Should().NotBeNull();
        projectFromDb.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public async Task ShouldNotChangeStatus_WhenMilestoneIsAlreadyApproved()
    {
        // Arrange
        var employerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 10000m,
            Currency = "USD",
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        var milestone = new ContractMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = _contract.Id,
            Description = "Already approved milestone",
            Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = ContractMilestoneStatus.Approved, // Already approved
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        await Context.AddAuditableAsync(employerWallet);
        await Context.AddAuditableAsync(milestone);
        await SaveChangesAsync();

        var request = new UpdContractMilestoneStatusModeratorVM
        {
            Status = ContractMilestoneStatus.InProgress
        };

        // Act
        var response = await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone.Id}/moderator", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify milestone status remains approved
        var unchangedMilestone = await Context.ContractMilestones
            .FirstOrDefaultAsync(x => x.Id == milestone.Id);
        unchangedMilestone.Should().NotBeNull();
        unchangedMilestone.Status.Should().Be(ContractMilestoneStatus.Approved);
    }

    [Fact]
    public async Task ShouldChangeStatusToInProgressForContractMilestone_WithoutCreatingTransactions()
    {
        // Arrange
        var employerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 10000m,
            Currency = "USD",
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };
        var freelancerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 0m,
            Currency = "USD",
            CreatedBy = _freelancerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _freelancerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        var milestone = new ContractMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = _contract.Id,
            Description = "Milestone for rejection",
            Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = ContractMilestoneStatus.Submitted,
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        await Context.AddAuditableAsync(employerWallet);
        await Context.AddAuditableAsync(freelancerWallet);
        await Context.AddAuditableAsync(milestone);
        await SaveChangesAsync();

        var request = new UpdContractMilestoneStatusModeratorVM
        {
            Status = ContractMilestoneStatus.InProgress
        };

        // Act
        var response = await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone.Id}/moderator", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verify milestone status changed to Rejected
        var updatedMilestone = await Context.ContractMilestones
            .FirstOrDefaultAsync(x => x.Id == milestone.Id);
        updatedMilestone.Should().NotBeNull();
        updatedMilestone.Status.Should().Be(request.Status);

        // Verify wallet balances remain unchanged (no transaction on rejection)
        var unchangedEmployerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _employerUser.Id);
        unchangedEmployerWallet!.Balance.Should().Be(employerWallet.Balance);

        var unchangedFreelancerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _freelancerUser.Id);
        unchangedFreelancerWallet!.Balance.Should().Be(freelancerWallet.Balance);
    }

    [Fact]
    public async Task ShouldChangeStatusToUnderReview_WithoutCreatingTransactions()
    {
        // Arrange
        var employerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 10000m,
            Currency = "USD",
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };
        var freelancerWallet = new UserWallet
        {
            Id = Guid.NewGuid(),
            Balance = 0m,
            Currency = "USD",
            CreatedBy = _freelancerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _freelancerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        var milestone = new ContractMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = _contract.Id,
            Description = "Milestone for review",
            Amount = 500m,
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = ContractMilestoneStatus.Submitted,
            CreatedBy = _employerUser.Id,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = _employerUser.Id,
            ModifiedAt = DateTime.UtcNow
        };

        await Context.AddAuditableAsync(employerWallet);
        await Context.AddAuditableAsync(freelancerWallet);
        await Context.AddAuditableAsync(milestone);
        await SaveChangesAsync();

        var request = new UpdContractMilestoneStatusModeratorVM
        {
            Status = ContractMilestoneStatus.UnderReview
        };

        // Act
        var response = await Client.PutAsJsonAsync($"ContractMilestone/status/{milestone.Id}/moderator", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        // Verify milestone status changed to UnderReview
        var updatedMilestone = await Context.ContractMilestones
            .FirstOrDefaultAsync(x => x.Id == milestone.Id);
        updatedMilestone.Should().NotBeNull();
        updatedMilestone.Status.Should().Be(ContractMilestoneStatus.UnderReview);

        // Verify wallet balances remain unchanged (no transaction on under review)
        var unchangedEmployerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _employerUser.Id);
        unchangedEmployerWallet!.Balance.Should().Be(10000m);

        var unchangedFreelancerWallet = await Context.Set<UserWallet>()
            .FirstOrDefaultAsync(w => w.CreatedBy == _freelancerUser.Id);
        unchangedFreelancerWallet!.Balance.Should().Be(0m);
    }

    public async Task InitializeAsync()
    {
        _moderatorUser = UserData.CreateTestUser(UserId, roleId: GetRoleIdByName(Settings.Roles.ModeratorRole));
        _employerUser = UserData.CreateTestUser(Guid.NewGuid());
        _freelancerUser = UserData.CreateTestUser(Guid.NewGuid());
        _freelancer = FreelancerData.CreateFreelancer(userId: _freelancerUser.Id);
        _project = ProjectData.CreateProject(userId: _employerUser.Id);
        _contract = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            agreedRate: 2000m,
            createdById: _employerUser.Id
        );
        _contractMilestone = new ContractMilestone
        {
            Id = Guid.NewGuid(),
            ContractId = _contract.Id,
            Description = "Test contract milestone",
            Amount = _contract.AgreedRate / 2,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = ContractMilestoneStatus.Pending,
            CreatedBy = _employerUser.Id
        };

        await Context.AddAuditableAsync(_moderatorUser);
        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_freelancerUser);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_contract);
        await Context.AddAuditableAsync(_contractMilestone);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}