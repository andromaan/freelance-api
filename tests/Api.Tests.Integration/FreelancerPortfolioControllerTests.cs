using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.Portfolio;
using DAL.Extensions;
using Domain.Models.Freelance;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class FreelancerPortfolioControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.FreelancerRole), IAsyncLifetime
{
    private User _freelancerUser = null!;
    private User _otherFreelancerUser = null!;
    private User _employerUser = null!;
    private Freelancer _freelancer = null!;
    private Freelancer _otherFreelancer = null!;
    private Portfolio _existingPortfolio = null!;

    [Fact]
    public async Task ShouldCreatePortfolio()
    {
        // Arrange
        var request = new CreatePortfolioVM
        {
            Title = "E-commerce Website",
            Description = "Built a full-featured e-commerce platform using React and Node.js",
            PortfolioUrl = "https://github.com/user/ecommerce-project"
        };

        // Act
        var response = await Client.PostAsJsonAsync("FreelancerPortfolio", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfolioFromResponse = await JsonHelper.GetPayloadAsync<PortfolioVM>(response);
        var portfolioId = portfolioFromResponse.Id;

        var portfolioFromDb = await Context.Portfolios.FirstOrDefaultAsync(x => x.Id == portfolioId);

        portfolioFromDb.Should().NotBeNull();
        portfolioFromDb.Title.Should().Be(request.Title);
        portfolioFromDb.Description.Should().Be(request.Description);
        portfolioFromDb.PortfolioUrl.Should().Be(request.PortfolioUrl);
        portfolioFromDb.FreelancerId.Should().Be(_freelancer.Id);
        portfolioFromDb.CreatedBy.Should().Be(_freelancerUser.Id);
    }

    [Fact]
    public async Task ShouldCreatePortfolioWithMinimalData()
    {
        // Arrange
        var request = new CreatePortfolioVM
        {
            Title = "Mobile App Project",
            Description = null,
            PortfolioUrl = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("FreelancerPortfolio", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfolioFromResponse = await JsonHelper.GetPayloadAsync<PortfolioVM>(response);
        var portfolioId = portfolioFromResponse.Id;

        var portfolioFromDb = await Context.Portfolios.FirstOrDefaultAsync(x => x.Id == portfolioId);

        portfolioFromDb.Should().NotBeNull();
        portfolioFromDb.Title.Should().Be(request.Title);
        portfolioFromDb.Description.Should().BeNull();
        portfolioFromDb.PortfolioUrl.Should().BeNull();
        portfolioFromDb.FreelancerId.Should().Be(_freelancer.Id);
    }

    [Fact]
    public async Task ShouldNotCreatePortfolioWithoutFreelancerRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _employerUser.Id);

        var request = new CreatePortfolioVM
        {
            Title = "Unauthorized Portfolio",
            Description = "This should not be created",
            PortfolioUrl = "https://example.com/portfolio"
        };

        // Act
        var response = await Client.PostAsJsonAsync("FreelancerPortfolio", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldGetPortfolioById()
    {
        // Act
        var response = await Client.GetAsync($"FreelancerPortfolio/{_existingPortfolio.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfolioFromResponse = await JsonHelper.GetPayloadAsync<PortfolioVM>(response);

        portfolioFromResponse.Id.Should().Be(_existingPortfolio.Id);
        portfolioFromResponse.Title.Should().Be(_existingPortfolio.Title);
        portfolioFromResponse.Description.Should().Be(_existingPortfolio.Description);
        portfolioFromResponse.PortfolioUrl.Should().Be(_existingPortfolio.PortfolioUrl);
    }

    [Fact]
    public async Task ShouldGetPortfoliosByFreelancerId()
    {
        // Arrange - portfolio створено в InitializeAsync

        // Act
        var response = await Client.GetAsync($"FreelancerPortfolio/get-by-freelancer/{_freelancer.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfoliosFromResponse = await JsonHelper.GetPayloadAsync<List<PortfolioVM>>(response);

        portfoliosFromResponse.Should().NotBeNull();
        portfoliosFromResponse.Should().HaveCountGreaterThan(0);
        portfoliosFromResponse.Should().Contain(p => p.Id == _existingPortfolio.Id);
    }

    [Fact]
    public async Task ShouldGetPortfoliosByFreelancerIdWithoutAuthentication()
    {
        // Arrange - встановлюємо клієнт без токена аутентифікації
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync($"FreelancerPortfolio/get-by-freelancer/{_freelancer.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfoliosFromResponse = await JsonHelper.GetPayloadAsync<List<PortfolioVM>>(response);

        portfoliosFromResponse.Should().NotBeNull();
        portfoliosFromResponse.Should().Contain(p => p.Id == _existingPortfolio.Id);
    }

    [Fact]
    public async Task ShouldReturnEmptyListForFreelancerWithoutPortfolios()
    {
        // Act
        var response = await Client.GetAsync($"FreelancerPortfolio/get-by-freelancer/{_otherFreelancer.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfoliosFromResponse = await JsonHelper.GetPayloadAsync<List<PortfolioVM>>(response);

        portfoliosFromResponse.Should().NotBeNull();
        portfoliosFromResponse.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldUpdatePortfolio()
    {
        // Arrange
        var request = new UpdatePortfolioVM
        {
            Title = "Updated Portfolio Title",
            Description = "Updated description with more details",
            PortfolioUrl = "https://github.com/user/updated-project"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"FreelancerPortfolio/{_existingPortfolio.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfolioFromDb = await Context.Portfolios.FirstOrDefaultAsync(x => x.Id == _existingPortfolio.Id);

        portfolioFromDb.Should().NotBeNull();
        portfolioFromDb.Title.Should().Be(request.Title);
        portfolioFromDb.Description.Should().Be(request.Description);
        portfolioFromDb.PortfolioUrl.Should().Be(request.PortfolioUrl);
    }

    [Fact]
    public async Task ShouldNotUpdatePortfolioOfAnotherFreelancer()
    {
        // Arrange
        var otherPortfolio = PortfolioData.CreatePortfolio(
            freelancerId: _otherFreelancer.Id,
            userId: _otherFreelancerUser.Id,
            title: "Other Freelancer Portfolio"
        );
        await Context.AddAuditableAsync(otherPortfolio);
        await SaveChangesAsync();

        var request = new UpdatePortfolioVM
        {
            Title = "Hacked Portfolio",
            Description = "This should not work",
            PortfolioUrl = "https://example.com/hack"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"FreelancerPortfolio/{otherPortfolio.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldNotUpdatePortfolioWithoutFreelancerRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _employerUser.Id);

        var request = new UpdatePortfolioVM
        {
            Title = "Unauthorized Update",
            Description = "This should not work",
            PortfolioUrl = "https://example.com/unauthorized"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"FreelancerPortfolio/{_existingPortfolio.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldDeletePortfolio()
    {
        // Arrange
        var portfolioToDelete = PortfolioData.CreatePortfolio(
            freelancerId: _freelancer.Id,
            userId: _freelancerUser.Id,
            title: "Portfolio to Delete"
        );
        await Context.AddAuditableAsync(portfolioToDelete);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"FreelancerPortfolio/{portfolioToDelete.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var portfolioFromDb = await Context.Portfolios.FirstOrDefaultAsync(x => x.Id == portfolioToDelete.Id);
        portfolioFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeletePortfolioOfAnotherFreelancer()
    {
        // Arrange
        var otherPortfolio = PortfolioData.CreatePortfolio(
            freelancerId: _otherFreelancer.Id,
            userId: _otherFreelancerUser.Id,
            title: "Other Freelancer Portfolio to Delete"
        );
        await Context.AddAuditableAsync(otherPortfolio);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"FreelancerPortfolio/{otherPortfolio.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var portfolioFromDb = await Context.Portfolios.FirstOrDefaultAsync(x => x.Id == otherPortfolio.Id);
        portfolioFromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotDeletePortfolioWithoutFreelancerRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _employerUser.Id);

        // Act
        var response = await Client.DeleteAsync($"FreelancerPortfolio/{_existingPortfolio.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenGettingNonExistentPortfolio()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"FreelancerPortfolio/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenUpdatingNonExistentPortfolio()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var request = new UpdatePortfolioVM
        {
            Title = "Non-existent Portfolio",
            Description = "This should return not found",
            PortfolioUrl = "https://example.com/notfound"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"FreelancerPortfolio/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenDeletingNonExistentPortfolio()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"FreelancerPortfolio/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenCreatingPortfolioWithEmptyTitle()
    {
        // Arrange
        var request = new CreatePortfolioVM
        {
            Title = "",
            Description = "Portfolio with empty title",
            PortfolioUrl = "https://example.com/portfolio"
        };

        // Act
        var response = await Client.PostAsJsonAsync("FreelancerPortfolio", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldCreateMultiplePortfoliosForSameFreelancer()
    {
        // Arrange
        var request1 = new CreatePortfolioVM
        {
            Title = "First Portfolio",
            Description = "First portfolio description",
            PortfolioUrl = "https://example.com/portfolio1"
        };

        var request2 = new CreatePortfolioVM
        {
            Title = "Second Portfolio",
            Description = "Second portfolio description",
            PortfolioUrl = "https://example.com/portfolio2"
        };

        // Act
        var response1 = await Client.PostAsJsonAsync("FreelancerPortfolio", request1);
        var response2 = await Client.PostAsJsonAsync("FreelancerPortfolio", request2);

        // Assert
        response1.IsSuccessStatusCode.Should().BeTrue();
        response2.IsSuccessStatusCode.Should().BeTrue();

        var portfolio1 = await JsonHelper.GetPayloadAsync<PortfolioVM>(response1);
        var portfolio2 = await JsonHelper.GetPayloadAsync<PortfolioVM>(response2);

        portfolio1.Id.Should().NotBe(portfolio2.Id);

        var portfoliosFromDb = await Context.Portfolios
            .Where(p => p.FreelancerId == _freelancer.Id)
            .ToListAsync();

        portfoliosFromDb.Should().HaveCountGreaterThanOrEqualTo(3); // Including _existingPortfolio
    }

    public async Task InitializeAsync()
    {
        _freelancerUser = UserData.CreateTestUser(
            id: UserId,
            email: "freelancer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.FreelancerRole)
        );

        _otherFreelancerUser = UserData.CreateTestUser(
            email: "otherfreelancer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.FreelancerRole)
        );

        _employerUser = UserData.CreateTestUser(
            email: "employer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.EmployerRole)
        );

        _freelancer = FreelancerData.CreateFreelancer(
            id: Guid.NewGuid(),
            userId: _freelancerUser.Id
        );

        _otherFreelancer = FreelancerData.CreateFreelancer(
            id: Guid.NewGuid(),
            userId: _otherFreelancerUser.Id
        );

        _existingPortfolio = PortfolioData.CreatePortfolio(
            freelancerId: _freelancer.Id,
            userId: _freelancerUser.Id,
            title: "Existing Test Portfolio",
            description: "This is an existing portfolio for testing",
            portfolioUrl: "https://github.com/test/existing-project"
        );

        await Context.AddAuditableAsync(_freelancerUser);
        await Context.AddAuditableAsync(_otherFreelancerUser);
        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_otherFreelancer);
        await Context.AddAuditableAsync(_existingPortfolio);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}