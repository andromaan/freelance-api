using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.Reviews;
using DAL.Extensions;
using Domain.Models.Contracts;
using Domain.Models.Freelance;
using Domain.Models.Projects;
using Domain.Models.Reviews;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class ReviewControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.EmployerRole), IAsyncLifetime
{
    private User _employerUser = null!;
    private User _freelancerUser = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private Contract _contract = null!;
    private Review _existingReview = null!;

    [Fact]
    public async Task ShouldCreateReview()
    {
        // Arrange
        Context.Set<Review>().RemoveRange(Context.Set<Review>());
        await SaveChangesAsync();

        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 4.5m,
            ReviewText = "Excellent freelancer, delivered quality work on time!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);
        var reviewId = reviewFromResponse.Id;

        var reviewFromDb = await Context.Set<Review>().FirstOrDefaultAsync(x => x.Id == reviewId);

        reviewFromDb.Should().NotBeNull();
        reviewFromDb.ContractId.Should().Be(_contract.Id);
        reviewFromDb.ReviewedUserId.Should().Be(_freelancerUser.Id);
        reviewFromDb.ReviewerRoleId.Should().Be(_employerUser.RoleId);
        reviewFromDb.Rating.Should().Be(request.Rating);
        reviewFromDb.ReviewText.Should().Be(request.ReviewText);
        reviewFromDb.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task ShouldCreateReviewByFreelancer()
    {
        SwitchUser(_freelancerUser.Role!.Name, _freelancerUser.Id);

        // Arrange
        Context.Set<Review>().RemoveRange(Context.Set<Review>());
        await SaveChangesAsync();

        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 4.5m,
            ReviewText = "Excellent employer, great to work with!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);
        var reviewId = reviewFromResponse.Id;

        var reviewFromDb = await Context.Set<Review>().FirstOrDefaultAsync(x => x.Id == reviewId);

        reviewFromDb.Should().NotBeNull();
        reviewFromDb.ContractId.Should().Be(_contract.Id);
        reviewFromDb.ReviewedUserId.Should().Be(_employerUser.Id);
        reviewFromDb.ReviewerRoleId.Should().Be(_freelancerUser.RoleId);
        reviewFromDb.Rating.Should().Be(request.Rating);
        reviewFromDb.ReviewText.Should().Be(request.ReviewText);
        reviewFromDb.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task ShouldUpdateReview()
    {
        // Arrange
        var request = new UpdateReviewVM
        {
            Rating = 5.0m,
            ReviewText = "Updated review - Outstanding work!"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Review/{_existingReview.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);

        var reviewFromDb = await Context.Set<Review>().FirstOrDefaultAsync(x => x.Id == reviewFromResponse.Id);

        reviewFromDb.Should().NotBeNull();
        reviewFromDb.Rating.Should().Be(5.0m);
        reviewFromDb.ReviewText.Should().Be("Updated review - Outstanding work!");
        reviewFromDb.ContractId.Should().Be(_existingReview.ContractId);
        reviewFromDb.ModifiedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task ShouldGetReviewById()
    {
        // Act
        var response = await Client.GetAsync($"Review/{_existingReview.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);
        reviewFromResponse.Id.Should().Be(_existingReview.Id);
        reviewFromResponse.ContractId.Should().Be(_existingReview.ContractId);
        reviewFromResponse.Rating.Should().Be(_existingReview.Rating);
        reviewFromResponse.ReviewText.Should().Be(_existingReview.ReviewText);
    }

    [Fact]
    public async Task ShouldDeleteReview()
    {
        // Act
        var response = await Client.DeleteAsync($"Review/{_existingReview.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromDb = await Context.Set<Review>().FirstOrDefaultAsync(x => x.Id == _existingReview.Id);
        reviewFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotCreateReviewWithEmptyText()
    {
        // Arrange
        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 4.0m,
            ReviewText = "" // Empty text
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateReviewWithRatingOutOfRange()
    {
        // Arrange
        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 6.0m, // Rating out of range (0-5)
            ReviewText = "Great work!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateReviewWithNegativeRating()
    {
        // Arrange
        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = -1.0m, // Negative rating
            ReviewText = "Great work!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateReviewWithEmptyText()
    {
        // Arrange
        var request = new UpdateReviewVM
        {
            Rating = 4.5m,
            ReviewText = "" // Empty text
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Review/{_existingReview.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateReviewWithRatingOutOfRange()
    {
        // Arrange
        var request = new UpdateReviewVM
        {
            Rating = 5.5m, // Rating out of range (0-5)
            ReviewText = "Updated review"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Review/{_existingReview.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotGetReviewByNonExistentId()
    {
        // Act
        var response = await Client.GetAsync($"Review/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateNonExistentReview()
    {
        // Arrange
        var request = new UpdateReviewVM
        {
            Rating = 4.0m,
            ReviewText = "Updated review"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Review/{Guid.NewGuid()}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotDeleteNonExistentReview()
    {
        // Act
        var response = await Client.DeleteAsync($"Review/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldCreateReviewWithMinimumRating()
    {
        // Arrange
        Context.Set<Review>().RemoveRange(Context.Set<Review>());
        await SaveChangesAsync();

        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 0.0m, // Minimum valid rating
            ReviewText = "Work needs significant improvement."
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);
        reviewFromResponse.Rating.Should().Be(0.0m);
    }

    [Fact]
    public async Task ShouldCreateReviewWithMaximumRating()
    {
        // Arrange
        Context.Set<Review>().RemoveRange(Context.Set<Review>());
        await SaveChangesAsync();

        var request = new CreateReviewVM
        {
            ContractId = _contract.Id,
            Rating = 5.0m, // Maximum valid rating
            ReviewText = "Perfect work in every way!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Review", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var reviewFromResponse = await JsonHelper.GetPayloadAsync<ReviewVM>(response);
        reviewFromResponse.Rating.Should().Be(5.0m);
    }

    public async Task InitializeAsync()
    {
        // Set employer user to the same UserId as the JWT token
        _employerUser = UserData.CreateTestUser(id: UserId, email: "employer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.EmployerRole));
        _freelancerUser = UserData.CreateTestUser(email: "freelancer@test.com",
            roleId: GetRoleIdByName(Settings.Roles.FreelancerRole));

        _project = ProjectData.CreateProject(userId: _employerUser.Id);
        _freelancer = FreelancerData.CreateFreelancer(userId: _freelancerUser.Id);

        _contract = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            agreedRate: 2000m,
            createdById: _employerUser.Id
        );
        _contract.Status = ContractStatus.Completed;

        _existingReview = ReviewData.CreateReview(
            contractId: _contract.Id,
            reviewedUserId: _freelancerUser.Id,
            rating: 4.0m,
            reviewText: "Good work overall.",
            reviewerRoleId: _employerUser.RoleId,
            createdById: _employerUser.Id
        );


        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_freelancerUser);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_contract);
        await Context.AddAuditableAsync(_existingReview);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}