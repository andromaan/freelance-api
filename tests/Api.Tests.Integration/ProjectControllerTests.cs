using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.Project;
using DAL.Extensions;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class ProjectControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private Project _project = null!;
    private User _employerUser = null!;

    [Fact]
    public async Task ShouldCreateProject()
    {
        // Arrange
        var projectTitle = "New Test Project";
        var request = new CreateProjectVM
        {
            Title = projectTitle,
            Description = "New Test Project Description",
            Budget = 10000m
        };

        // Act
        var response = await Client.PostAsJsonAsync("Project", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projectFromResponse = await JsonHelper.GetPayloadAsync<ProjectVM>(response);
        var projectId = projectFromResponse.Id;

        var projectFromDb = await Context.Set<Project>().FirstOrDefaultAsync(x => x.Id == projectId);

        projectFromDb.Should().NotBeNull();
        projectFromDb.Title.Should().Be(projectTitle);
        projectFromDb.Description.Should().Be(request.Description);
        projectFromDb.Budget.Should().Be(request.Budget);
        projectFromDb.Status.Should().Be(ProjectStatus.Open);
    }

    [Fact]
    public async Task ShouldUpdateProject()
    {
        // Arrange
        var projectTitle = "Updated Project";
        var request = new UpdateProjectVM
        {
            Title = projectTitle,
            Description = "Updated Description",
            Budget = 15000m
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Project/{_project.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projectFromResponse = await JsonHelper.GetPayloadAsync<ProjectVM>(response);

        var projectFromDb = await Context.Set<Project>().FirstOrDefaultAsync(x => x.Id == projectFromResponse.Id);

        projectFromDb.Should().NotBeNull();
        projectFromDb.Title.Should().Be(projectTitle);
        projectFromDb.Description.Should().Be(request.Description);
        projectFromDb.Budget.Should().Be(request.Budget);
    }

    [Fact]
    public async Task ShouldDeleteProject()
    {
        // Act
        var response = await Client.DeleteAsync($"Project/{_project.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projectFromDb = await Context.Set<Project>().FirstOrDefaultAsync(x => x.Id == _project.Id);

        projectFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetProjectById()
    {
        // Act
        var response = await Client.GetAsync($"Project/{_project.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projectFromResponse = await JsonHelper.GetPayloadAsync<ProjectVM>(response);

        projectFromResponse.Should().NotBeNull();
        projectFromResponse.Id.Should().Be(_project.Id);
        projectFromResponse.Title.Should().Be(_project.Title);
    }

    [Fact]
    public async Task ShouldNotUpdateBecauseNotFound()
    {
        // Arrange
        var request = new UpdateProjectVM
        {
            Title = "Test",
            Description = "Test",
            Budget = 5000m
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Project/{Guid.NewGuid()}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotDeleteBecauseNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"Project/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotGetByIdBecauseNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Project/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldGetAllProjects()
    {
        // Act
        var response = await Client.GetAsync("Project");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projects = await JsonHelper.GetPayloadAsync<List<ProjectVM>>(response);

        projects.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ShouldGetProjectsByEmployer()
    {
        // Act
        var response = await Client.GetAsync("Project/by-employer");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projects = await JsonHelper.GetPayloadAsync<List<ProjectVM>>(response);

        projects.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldUpdateProjectCategories()
    {
        // Arrange
        var category1 = new Category { Id = 0, Name = "TestCategory1" };
        var category2 = new Category { Id = -1, Name = "TestCategory2" };
        await Context.AddAsync(category1);
        await Context.AddAsync(category2);
        await SaveChangesAsync();

        var request = new UpdateProjectCategoriesVM
        {
            CategoryIds = new List<int> { category1.Id, category2.Id }
        };

        // Act
        var response = await Client.PatchAsync($"Project/categories/{_project.Id}",
            JsonContent.Create(request));

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var projectFromDb = await Context.Set<Project>()
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(x => x.Id == _project.Id);

        projectFromDb!.Categories.Should().HaveCount(2);
        projectFromDb.Categories.Should().Contain(c => c.Id == category1.Id);
        projectFromDb.Categories.Should().Contain(c => c.Id == category2.Id);
    }

    public async Task InitializeAsync()
    {
        _employerUser = UserData.CreateTestUser(UserId, roleId: GetRoleIdByName(Settings.Roles.EmployerRole));
        _project = ProjectData.CreateProject(userId: UserId);

        await Context.AddAuditableAsync(_employerUser);
        await Context.AddAuditableAsync(_project);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}