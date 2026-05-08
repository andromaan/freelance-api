using System.Net;
using System.Net.Http.Json;
using BLL.ViewModels.Quote;
using DAL.Extensions;
using Domain.Models.Freelance;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class QuoteControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private Quote _quote = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private User _user = null!;

    [Fact]
    public async Task ShouldCreateQuote()
    {
        // Arrange
        var request = new CreateQuoteVM
        {
            ProjectId = _project.Id,
            Amount = 2000m,
            Message = "My quote for this project"
        };
        
        Context.Remove(_quote);
        await SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync("Quote", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quoteFromResponse = await JsonHelper.GetPayloadAsync<QuoteVM>(response);
        var quoteId = quoteFromResponse.Id;

        var quoteFromDb = await Context.Set<Quote>().FirstOrDefaultAsync(x => x.Id == quoteId);

        quoteFromDb.Should().NotBeNull();
        quoteFromDb.ProjectId.Should().Be(_project.Id);
        quoteFromDb.Amount.Should().Be(2000m);
        quoteFromDb.Message.Should().Be("My quote for this project");
    }
    
    [Fact]
    public async Task ShouldNotCreateMoreThanOneQuotePerFreelancerPerProject()
    {
        // Arrange
        var request = new CreateQuoteVM
        {
            ProjectId = _project.Id,
            Amount = 1500m,
            Message = "I can do this project"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Quote", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldUpdateQuote()
    {
        // Arrange
        var request = new UpdateQuoteVM
        {
            Amount = 3000m,
            Message = "Updated quote message"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Quote/{_quote.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quoteFromResponse = await JsonHelper.GetPayloadAsync<QuoteVM>(response);

        var quoteFromDb = await Context.Set<Quote>().FirstOrDefaultAsync(x => x.Id == quoteFromResponse.Id);

        quoteFromDb.Should().NotBeNull();
        quoteFromDb.Amount.Should().Be(3000m);
        quoteFromDb.Message.Should().Be("Updated quote message");
    }

    [Fact]
    public async Task ShouldDeleteQuote()
    {
        // Act
        var response = await Client.DeleteAsync($"Quote/{_quote.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quoteFromDb = await Context.Set<Quote>().FirstOrDefaultAsync(x => x.Id == _quote.Id);

        quoteFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetQuoteById()
    {
        // Act
        var response = await Client.GetAsync($"Quote/{_quote.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quoteFromResponse = await JsonHelper.GetPayloadAsync<QuoteVM>(response);

        quoteFromResponse.Should().NotBeNull();
        quoteFromResponse.Id.Should().Be(_quote.Id);
        quoteFromResponse.ProjectId.Should().Be(_project.Id);
    }

    [Fact]
    public async Task ShouldGetQuotesByProjectId()
    {
        // Act
        var response = await Client.GetAsync($"Quote/by-project/{_project.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quotes = await JsonHelper.GetPayloadAsync<List<QuoteVM>>(response);

        quotes.Should().NotBeEmpty();
        quotes.Should().Contain(q => q.Id == _quote.Id);
    }

    [Fact]
    public async Task ShouldNotCreateQuoteBecauseProjectNotFound()
    {
        // Arrange
        var request = new CreateQuoteVM
        {
            ProjectId = Guid.NewGuid(),
            Amount = 2000m,
            Message = "Test"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Quote", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotCreateQuoteBecauseAmountIsGreaterThanProjectBudget()
    {
        // Arrange
        var request = new CreateQuoteVM
        {
            ProjectId = _project.Id,
            Amount = 100000m, // Assuming this is greater than the project budget
            Message = "Test"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Quote", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateBecauseNotFound()
    {
        // Arrange
        var request = new UpdateQuoteVM
        {
            Amount = 1000m,
            Message = "Test"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Quote/{Guid.NewGuid()}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotDeleteBecauseNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"Quote/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotGetByIdBecauseNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Quote/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnEmptyListForProjectWithNoQuotes()
    {
        // Arrange
        var projectWithoutQuotes = ProjectData.CreateProject(userId: _user.Id);
        await Context.AddAuditableAsync(projectWithoutQuotes);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"Quote/by-project/{projectWithoutQuotes.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var quotes = await JsonHelper.GetPayloadAsync<List<QuoteVM>>(response);

        quotes.Should().BeEmpty();
    }

    public async Task InitializeAsync()
    {
        _user = UserData.CreateTestUser(UserId);
        _project = ProjectData.CreateProject(userId: _user.Id);
        _freelancer = FreelancerData.CreateFreelancer(userId: _user.Id);
        _quote = new Quote
        {
            Id = Guid.NewGuid(),
            ProjectId = _project.Id,
            FreelancerId = _freelancer.Id,
            Amount = 1500m,
            Message = "Test quote message"
        };

        await Context.AddAuditableAsync(_user);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_quote);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}