using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.Services.PasswordHasher;
using BLL.ViewModels.Auth;
using DAL.Extensions;
using Domain.Models.Users;
using FluentAssertions;
using Tests.Common;

namespace Api.Tests.Integration;

public class AccountControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, useJwtToken: false), IAsyncLifetime
{
    private User _existingUser = null!;
    private const string ExistingUserPassword = "Test123!@#";
    private const string ExistingUserEmail = "existing@test.com";

    // [Fact]
    // public async Task SignUp_ShouldCreateFreelancer_WhenValidData()
    // {
    //     // Arrange
    //     var request = new SignUpVM
    //     {
    //         Email = "newfreelancer@test.com",
    //         Password = "Test123!@#",
    //         DisplayName = "New Freelancer",
    //         UserRole = Settings.Roles.FreelancerRole
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("Account/sign-up", request);
    //
    //     // Assert
    //     response.IsSuccessStatusCode.Should().BeTrue();
    //
    //     var userFromDb = await Context.Set<User>()
    //         .Include(u => u.Role)
    //         .FirstOrDefaultAsync(x => x.Email == request.Email);
    //
    //     userFromDb.Should().NotBeNull();
    //     userFromDb.Email.Should().Be(request.Email);
    //     userFromDb.DisplayName.Should().Be(request.DisplayName);
    //     userFromDb.Role!.Name.Should().Be(Settings.Roles.FreelancerRole);
    // }

    // [Fact]
    // public async Task SignUp_ShouldCreateEmployer_WhenValidData()
    // {
    //     // Arrange
    //     var request = new SignUpVM
    //     {
    //         Email = "newemployer@test.com",
    //         Password = "Test123!@#",
    //         DisplayName = "New Employer",
    //         UserRole = Settings.Roles.EmployerRole
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("Account/sign-up", request);
    //
    //     // Assert
    //     response.IsSuccessStatusCode.Should().BeTrue();
    //
    //     var userFromDb = await Context.Set<User>()
    //         .Include(u => u.Role)
    //         .FirstOrDefaultAsync(x => x.Email == request.Email);
    //
    //     userFromDb.Should().NotBeNull();
    //     userFromDb.Email.Should().Be(request.Email);
    //     userFromDb.DisplayName.Should().Be(request.DisplayName);
    //     userFromDb.Role!.Name.Should().Be(Settings.Roles.EmployerRole);
    // }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new SignUpVM
        {
            Email = ExistingUserEmail,
            Password = "Test123!@#",
            DisplayName = "Test User",
            UserRole = Settings.Roles.FreelancerRole
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-up", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequest_WhenInvalidRole()
    {
        // Arrange
        var request = new SignUpVM
        {
            Email = "invalidrole@test.com",
            Password = "Test123!@#",
            DisplayName = "Test User",
            UserRole = "invalid_role"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-up", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequest_WhenAdminRoleRequested()
    {
        // Arrange
        var request = new SignUpVM
        {
            Email = "wannabeadmin@test.com",
            Password = "Test123!@#",
            DisplayName = "Wannabe Admin",
            UserRole = Settings.Roles.AdminRole
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-up", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // [Fact]
    // public async Task SignUp_ShouldReturnTokens_WhenSuccessful()
    // {
    //     // Arrange
    //     var request = new SignUpVM
    //     {
    //         Email = "tokentest@test.com",
    //         Password = "Test123!@#",
    //         DisplayName = "Token Test User",
    //         UserRole = Settings.Roles.FreelancerRole
    //     };
    //
    //     // Act
    //     var response = await Client.PostAsJsonAsync("Account/sign-up", request);
    //
    //     // Assert
    //     response.IsSuccessStatusCode.Should().BeTrue();
    //
    //     var content = await response.Content.ReadAsStringAsync();
    //     content.Should().Contain("accessToken");
    //     content.Should().Contain("refreshToken");
    // }

    [Fact]
    public async Task SignIn_ShouldReturnOk_WhenValidCredentials()
    {
        // Arrange
        var request = new SignInVM
        {
            Email = ExistingUserEmail,
            Password = ExistingUserPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-in", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SignIn_ShouldReturnTokens_WhenSuccessful()
    {
        // Arrange
        var request = new SignInVM
        {
            Email = ExistingUserEmail,
            Password = ExistingUserPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-in", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("accessToken");
        content.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task SignIn_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var request = new SignInVM
        {
            Email = "nonexistent@test.com",
            Password = "Test123!@#"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignIn_ShouldReturnBadRequest_WhenWrongPassword()
    {
        // Arrange
        var request = new SignInVM
        {
            Email = ExistingUserEmail,
            Password = "WrongPassword123!@#"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Account/sign-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        // Створюємо існуючого користувача безпосередньо в БД для тестів логіну
        var passwordHasher = new PasswordHasher();

        var userId = Guid.NewGuid();
        _existingUser = new User
        {
            Id = userId,
            Email = ExistingUserEmail,
            PasswordHash = passwordHasher.HashPassword(ExistingUserPassword),
            DisplayName = "Existing User",
            RoleId = GetRoleIdByName(Settings.Roles.FreelancerRole),
            CreatedBy = userId
        };

        await Context.AddAuditableAsync(_existingUser);
        
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}