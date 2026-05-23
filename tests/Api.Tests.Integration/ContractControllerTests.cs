using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.Contract;
using DAL.Extensions;
using Domain.Models.Contracts;
using Domain.Models.Freelance;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class ContractControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.EmployerRole), IAsyncLifetime
{
    private User _employerUser = null!;
    private User _freelancerUser = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private Quote _quote = null!;
    private Contract _contract = null!;
    private ProjectMilestone _projectMilestone = null!;

    [Fact]
    public async Task ShouldCreateContract()
    {
        // Arrange
        Context.Contracts.Remove(_contract);
        
        var newQuote = QuoteData.CreateQuote(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            amount: 3000m
        );
        await Context.AddAuditableAsync(newQuote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{newQuote.Id}", new { });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var contractFromResponse = await JsonHelper.GetPayloadAsync<ContractVM>(response);
        var contractId = contractFromResponse.Id;

        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contractId);

        contractFromDb.Should().NotBeNull();
        contractFromDb.ProjectId.Should().Be(_project.Id);
        contractFromDb.FreelancerId.Should().Be(_freelancer.Id);
        contractFromDb.AgreedRate.Should().Be(3000m);
        contractFromDb.Status.Should().Be(ContractStatus.Pending);
    }

    [Fact]
    public async Task ShouldCreateContractWithMilestones()
    {
        // Arrange
        var newProject = ProjectData.CreateProject(userId: _employerUser.Id);
        var milestone1 = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = newProject.Id,
            Description = "Milestone 1",
            DueDate = DateTime.UtcNow.AddDays(30),
            Amount = 1000m,
            CreatedBy = _employerUser.Id
        };
        var milestone2 = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = newProject.Id,
            Description = "Milestone 2",
            DueDate = DateTime.UtcNow.AddDays(60),
            Amount = 2000m,
            CreatedBy = _employerUser.Id
        };
        var newQuote = QuoteData.CreateQuote(
            projectId: newProject.Id,
            freelancerId: _freelancer.Id,
            amount: 3000m
        );

        await Context.AddAuditableAsync(newProject);
        await Context.AddAuditableAsync(milestone1);
        await Context.AddAuditableAsync(milestone2);
        await Context.AddAuditableAsync(newQuote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{newQuote.Id}", new { });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var contractFromResponse = await JsonHelper.GetPayloadAsync<ContractVM>(response);
        var contractId = contractFromResponse.Id;

        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contractId);
        contractFromDb.Should().NotBeNull();

        var contractMilestones = await Context.ContractMilestones
            .Where(cm => cm.ContractId == contractId)
            .ToListAsync();

        contractMilestones.Should().HaveCount(2);
        contractMilestones.Should().Contain(cm => cm.Description == "Milestone 1" && cm.Amount == 1000m);
        contractMilestones.Should().Contain(cm => cm.Description == "Milestone 2" && cm.Amount == 2000m);
        contractMilestones.All(cm => cm.Status == ContractMilestoneStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldUpdateContract()
    {
        // Arrange
        var newStartDate = DateTime.UtcNow.AddDays(5);
        var newEndDate = DateTime.UtcNow.AddDays(35);
        var request = new UpdateContractVM 
        { 
            StartDate = newStartDate,
            EndDate = newEndDate
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Contract?contractId={_contract.Id}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var contractFromResponse = await JsonHelper.GetPayloadAsync<ContractVM>(response);
        
        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contractFromResponse.Id);
        
        contractFromDb.Should().NotBeNull();
        contractFromDb.StartDate.Should().BeCloseTo(newStartDate, TimeSpan.FromSeconds(1));
        contractFromDb.EndDate.Should().BeCloseTo(newEndDate, TimeSpan.FromSeconds(1));
        contractFromDb.AgreedRate.Should().Be(contractFromResponse.AgreedRate);
    }

    [Fact]
    public async Task ShouldNotCreateContractBecauseQuoteNotFound()
    {
        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{Guid.NewGuid()}", new { });
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateContractBecauseNotFound()
    {
        // Arrange
        var request = new UpdateContractVM 
        { 
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"Contract?contractId={Guid.NewGuid()}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateContractWithEndDateBeforeStartDate()
    {
        // Arrange
        var request = new UpdateContractVM 
        { 
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow, // End date before start date
        };
        
        // Act
        var response = await Client.PutAsJsonAsync($"Contract?contractId={_contract.Id}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldCreateContractWithDeadlineFromProject()
    {
        // Arrange - Create project without milestones
        var projectWithDeadline = ProjectData.CreateProject(userId: _employerUser.Id);
        projectWithDeadline.Deadline = DateTime.UtcNow.AddDays(45);
        
        var newQuote = QuoteData.CreateQuote(
            projectId: projectWithDeadline.Id,
            freelancerId: _freelancer.Id,
            amount: 2500m
        );

        await Context.AddAuditableAsync(projectWithDeadline);
        await Context.AddAuditableAsync(newQuote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{newQuote.Id}", new { });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var contractFromResponse = await JsonHelper.GetPayloadAsync<ContractVM>(response);
        var contractId = contractFromResponse.Id;

        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contractId);
        
        contractFromDb.Should().NotBeNull();
        contractFromDb.EndDate.Should().BeCloseTo(projectWithDeadline.Deadline, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ShouldSetContractEndDateFromLatestMilestone()
    {
        // Arrange
        var newProject = ProjectData.CreateProject(userId: _employerUser.Id);
        newProject.Deadline = DateTime.UtcNow.AddDays(30);
        
        var milestone1 = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = newProject.Id,
            Description = "Early Milestone",
            DueDate = DateTime.UtcNow.AddDays(20),
            Amount = 1000m,
            CreatedBy = _employerUser.Id
        };
        var milestone2 = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = newProject.Id,
            Description = "Late Milestone",
            DueDate = DateTime.UtcNow.AddDays(60), // Later than project deadline
            Amount = 2000m,
            CreatedBy = _employerUser.Id
        };
        
        var newQuote = QuoteData.CreateQuote(
            projectId: newProject.Id,
            freelancerId: _freelancer.Id,
            amount: 3000m
        );

        await Context.AddAuditableAsync(newProject);
        await Context.AddAuditableAsync(milestone1);
        await Context.AddAuditableAsync(milestone2);
        await Context.AddAuditableAsync(newQuote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{newQuote.Id}", new { });

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var contractFromResponse = await JsonHelper.GetPayloadAsync<ContractVM>(response);
        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == contractFromResponse.Id);
        
        contractFromDb.Should().NotBeNull();
        // EndDate should be from the latest milestone, not the project deadline
        contractFromDb.EndDate.Should().BeCloseTo(milestone2.DueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ShouldUpdateContractStatus()
    {
        // Arrange
        var request = new UpdateContractStatusVM
        {
            Status = ContractStatus.Active // Use a valid status different from current
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Contract/update-status/{_contract.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var contractFromDb = await Context.Contracts.FirstOrDefaultAsync(x => x.Id == _contract.Id);
        contractFromDb.Should().NotBeNull();
        contractFromDb.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public async Task ShouldNotUpdateContractStatusWithInvalidStatus()
    {
        // Arrange
        var request = new UpdateContractStatusVM
        {
            Status = (ContractStatus)999 // Invalid enum value
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Contract/update-status/{_contract.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldUpdateProjectStatusToInProgress_WhenContractCreated()
    {
        // Arrange
        var project = ProjectData.CreateProject(userId: _employerUser.Id);
        await Context.AddAuditableAsync(project);
        var freelancer = FreelancerData.CreateFreelancer(userId: _freelancerUser.Id);
        await Context.AddAuditableAsync(freelancer);
        var quote = QuoteData.CreateQuote(projectId: project.Id, freelancerId: freelancer.Id, amount: 1000m);
        await Context.AddAuditableAsync(quote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"Contract/{quote.Id}", new { });
        response.IsSuccessStatusCode.Should().BeTrue();

        // Assert
        var projectFromDb = await Context.Set<Project>().FirstOrDefaultAsync(x => x.Id == project.Id);
        projectFromDb.Should().NotBeNull();
        projectFromDb.Status.Should().Be(ProjectStatus.InProgress);
    }

    public async Task InitializeAsync()
    {
        _employerUser = UserData.CreateTestUser(UserId, "employer@test.com");
        _freelancerUser = UserData.CreateTestUser(email: "freelancer@test.com");
        _project = ProjectData.CreateProject(userId: _employerUser.Id);
        _project.Deadline = DateTime.UtcNow.AddDays(30);
        _freelancer = FreelancerData.CreateFreelancer(userId: _freelancerUser.Id);
        _quote = QuoteData.CreateQuote(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            amount: 2000m
        );
        _contract = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            agreedRate: 2000m,
            createdById: _employerUser.Id
        );
        _projectMilestone = new ProjectMilestone
        {
            Id = Guid.NewGuid(),
            ProjectId = _project.Id,
            Description = "Test Milestone",
            DueDate = DateTime.UtcNow.AddDays(15),
            Amount = 1000m,
            CreatedBy = _employerUser.Id
        };

        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_freelancerUser);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_projectMilestone);
        await Context.AddAuditableAsync(_quote);
        await Context.AddAuditableAsync(_contract);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}
