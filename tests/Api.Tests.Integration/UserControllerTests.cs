using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.User;
using BLL.ViewModels.UserLanguage;
using DAL.Extensions;
using Domain.Models.Auth;
using Domain.Models.Countries;
using Domain.Models.Languages;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class UserControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory, customRole: Settings.Roles.AdminRole), IAsyncLifetime
{
    private User _testUser = null!;
    private User _adminUser = null!;
    private Country _country = null!;
    private Language _language1 = null!;
    private Language _language2 = null!;
    private Role _freelancerRole = null!;
    private Role _employerRole = null!;
    private Role _moderatorRole = null!;
    private Role _adminRole = null!;

    [Fact]
    public async Task ShouldCreateUser()
    {
        // Arrange
        var request = new CreateUserByAdminVM
        {
            Email = "newuser@test.com",
            Password = "Test123!@#",
            RoleId = _employerRole.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromResponse = await JsonHelper.GetPayloadAsync<UserVM>(response);
        var userId = userFromResponse.Id;

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == userId);

        userFromDb.Should().NotBeNull();
        userFromDb.Email.Should().Be(request.Email);
        userFromDb.RoleId.Should().Be(request.RoleId);
        userFromDb.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldNotCreateUserWithoutAdminRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _testUser.Id);

        var request = new CreateUserByAdminVM
        {
            Email = "unauthorized@test.com",
            Password = "Test123!@#",
            RoleId = _freelancerRole.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldGetMyselfAsUser()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _testUser.Id);

        var userLanguage1 = new UserLanguage
        {
            UserId = _testUser.Id,
            LanguageId = _language1.Id,
            ProficiencyLevel = ProficiencyLevel.Advanced
        };

        var userLanguage2 = new UserLanguage
        {
            UserId = _testUser.Id,
            LanguageId = _language2.Id,
            ProficiencyLevel = ProficiencyLevel.Intermediate
        };

        await Context.AddAsync(userLanguage1);
        await Context.AddAsync(userLanguage2);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("User/get-myself");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromResponse = await JsonHelper.GetPayloadAsync<UserVM>(response);

        userFromResponse.Id.Should().Be(_testUser.Id);
        userFromResponse.Email.Should().Be(_testUser.Email);
        userFromResponse.RoleId.Should().Be(_testUser.RoleId);
        userFromResponse.Languages.Should().HaveCount(2);
        userFromResponse.Languages.Should().Contain(l =>
            l.LanguageId == _language1.Id && l.ProficiencyLevel == nameof(ProficiencyLevel.Advanced));
        userFromResponse.Languages.Should().Contain(l =>
            l.LanguageId == _language2.Id && l.ProficiencyLevel == nameof(ProficiencyLevel.Intermediate));
    }

    [Fact]
    public async Task ShouldGetUserById()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        // Act
        var response = await Client.GetAsync($"User/{_testUser.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromResponse = await JsonHelper.GetPayloadAsync<UserVM>(response);

        userFromResponse.Id.Should().Be(_testUser.Id);
        userFromResponse.Email.Should().Be(_testUser.Email);
        userFromResponse.RoleId.Should().Be(_testUser.RoleId);
    }

    // TODO: make user by id getter for admin that returns user with all info about this user
    // [Fact]
    // public async Task ShouldNotGetUserByIdWithoutAdminRole()
    // {
    //     // Arrange
    //     SwitchUser(role: Settings.Roles.FreelancerRole, userId: _testUser.Id);
    //
    //     // Act
    //     var response = await Client.GetAsync($"User/{_testUser.Id}");
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    // }

    [Fact]
    public async Task ShouldGetMyself()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _testUser.Id);

        // Act
        var response = await Client.GetAsync("User/get-myself");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromResponse = await JsonHelper.GetPayloadAsync<UserVM>(response);

        userFromResponse.Id.Should().Be(_testUser.Id);
        userFromResponse.Email.Should().Be(_testUser.Email);
        userFromResponse.RoleId.Should().Be(_employerRole.Id);
    }

    [Fact]
    public async Task ShouldGetAllUsers()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        // Act
        var response = await Client.GetAsync("User");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var usersFromResponse = await JsonHelper.GetPayloadAsync<List<UserVM>>(response);

        usersFromResponse.Should().NotBeNull();
        usersFromResponse.Should().HaveCountGreaterThan(0);
        usersFromResponse.Should().Contain(u => u.Id == _testUser.Id);
    }

    [Fact]
    public async Task ShouldNotGetAllUsersWithoutAdminRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.FreelancerRole, userId: _testUser.Id);

        // Act
        var response = await Client.GetAsync("User");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldNotUpdateUserWithoutAdminRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.EmployerRole, userId: _testUser.Id);

        var request = new UpdateUserByAdminVM
        {
            Email = "hacker@test.com"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldDeleteUser()
    {
        // Arrange
        var userToDelete = UserData.CreateTestUser(
            email: "delete@test.com",
            roleId: _freelancerRole.Id
        );
        await Context.AddAuditableAsync(userToDelete);
        await SaveChangesAsync();

        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        // Act
        var response = await Client.DeleteAsync($"User/{userToDelete.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == userToDelete.Id);
        userFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteUserWithoutAdminRole()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.FreelancerRole, userId: _testUser.Id);

        // Act
        var response = await Client.DeleteAsync($"User/{_testUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenGettingNonExistentUser()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        // Act
        var response = await Client.GetAsync($"User/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenUpdatingNonExistentUser()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new UpdateUserByAdminVM
        {
            Email = "nonexistent@test.com"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenDeletingNonExistentUser()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        // Act
        var response = await Client.DeleteAsync($"User/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenCreatingUserWithExistingEmail()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new CreateUserByAdminVM
        {
            Email = _testUser.Email, // Existing email
            Password = "Test123!@#",
            RoleId = _freelancerRole.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ShouldUpdateUserEmail()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var newEmail = "newemail@test.com";
        var request = new UpdateUserByAdminVM
        {
            Email = newEmail
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == _testUser.Id);
        userFromDb.Should().NotBeNull();
        userFromDb.Email.Should().Be(newEmail);
    }

    [Fact]
    public async Task ShouldUpdateUserPassword()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var oldPasswordHash = _testUser.PasswordHash;
        var request = new UpdateUserByAdminVM
        {
            Password = "NewPassword123!@#"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == _testUser.Id);
        userFromDb.Should().NotBeNull();
        userFromDb.PasswordHash.Should().NotBe(oldPasswordHash);
        userFromDb.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldUpdateUserDisplayName()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var newDisplayName = "John Doe Updated";
        var request = new UpdateUserByAdminVM
        {
            DisplayName = newDisplayName
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == _testUser.Id);
        userFromDb.Should().NotBeNull();
        userFromDb.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public async Task ShouldNotUpdateUserEmailToExistingEmail()
    {
        // Arrange
        var anotherUser = UserData.CreateTestUser(
            email: "another@test.com",
            roleId: _freelancerRole.Id
        );
        await Context.AddAuditableAsync(anotherUser);
        await SaveChangesAsync();

        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new UpdateUserByAdminVM
        {
            Email = anotherUser.Email // Try to use existing email
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldUpdateMultipleUserFieldsAtOnce()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new UpdateUserByAdminVM
        {
            Email = "updatedemail@test.com",
            DisplayName = "Updated Name",
            Password = "NewPass123!@#",
            CountryId = _country.Id
        };

        // Act
        var response = await Client.PutAsJsonAsync($"User/{_testUser.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == _testUser.Id);
        userFromDb.Should().NotBeNull();
        userFromDb.Email.Should().Be(request.Email);
        userFromDb.DisplayName.Should().Be(request.DisplayName);
        userFromDb.CountryId.Should().Be(_country.Id);
        userFromDb.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldCreateUserWithDisplayName()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new CreateUserByAdminVM
        {
            Email = "userwithname@test.com",
            Password = "Test123!@#",
            RoleId = _freelancerRole.Id,
            DisplayName = "John Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromResponse = await JsonHelper.GetPayloadAsync<UserVM>(response);
        userFromResponse.DisplayName.Should().Be(request.DisplayName);

        var userFromDb = await Context.Set<User>().FirstOrDefaultAsync(x => x.Id == userFromResponse.Id);
        userFromDb.Should().NotBeNull();
        userFromDb.DisplayName.Should().Be(request.DisplayName);
    }


    [Fact]
    public async Task ShouldReturnBadRequestWhenCreatingUserWithInvalidEmail()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new CreateUserByAdminVM
        {
            Email = "invalid-email", // Invalid email format
            Password = "Test123!@#",
            RoleId = _freelancerRole.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenCreatingUserWithEmptyPassword()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var request = new CreateUserByAdminVM
        {
            Email = "validuser@test.com",
            Password = "", // Empty password
            RoleId = _freelancerRole.Id
        };

        // Act
        var response = await Client.PostAsJsonAsync("User", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldCreateUserLanguages()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var request1 = new CreateUserLanguageVM
        {
            LanguageId = _language1.Id,
            ProficiencyLevel = ProficiencyLevel.Advanced
        };

        var request2 = new CreateUserLanguageVM
        {
            LanguageId = _language2.Id,
            ProficiencyLevel = ProficiencyLevel.Intermediate
        };

        // Act
        var response1 = await Client.PostAsJsonAsync("User/languages", request1);
        var response2 = await Client.PostAsJsonAsync("User/languages", request2);

        // Assert
        response1.IsSuccessStatusCode.Should().BeTrue();
        response2.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>()
            .Include(f => f.Languages)
            .FirstOrDefaultAsync(x => x.Id == _testUser.Id);

        userFromDb.Should().NotBeNull();
        userFromDb.Languages.Should().HaveCount(2);
        userFromDb.Languages.Should().Contain(l =>
            l.LanguageId == request1.LanguageId && l.ProficiencyLevel == request1.ProficiencyLevel);
        userFromDb.Languages.Should().Contain(l =>
            l.LanguageId == request2.LanguageId && l.ProficiencyLevel == request2.ProficiencyLevel);
    }

    [Fact]
    public async Task ShouldNotCreateUserLanguageWithInvalidProficiencyLevel()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var request = new CreateUserLanguageVM
        {
            LanguageId = _language1.Id,
            ProficiencyLevel = (ProficiencyLevel)int.MaxValue // Invalid proficiency level
        };

        // Act
        var response = await Client.PostAsJsonAsync("User/languages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateUserLanguageWithInvalidLanguageId()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var request = new CreateUserLanguageVM
        {
            LanguageId = int.MaxValue, // Non-existent language ID
            ProficiencyLevel = ProficiencyLevel.Advanced
        };

        // Act
        var response = await Client.PostAsJsonAsync("User/languages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldUpdateUserLanguage()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var userLanguage = new UserLanguage
        {
            UserId = _testUser.Id,
            LanguageId = _language1.Id,
            ProficiencyLevel = ProficiencyLevel.Advanced
        };
        await Context.AddAsync(userLanguage);
        await SaveChangesAsync();

        var updateRequest = new UpdateUserLanguageVM
        {
            LanguageId = _language1.Id,
            ProficiencyLevel = ProficiencyLevel.Intermediate
        };

        // Act
        var response = await Client.PutAsJsonAsync("User/languages", updateRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>()
            .Include(f => f.Languages)
            .FirstOrDefaultAsync(x => x.Id == _testUser.Id);

        userFromDb.Should().NotBeNull();
        userFromDb.Languages.Should().HaveCount(1);
        userFromDb.Languages.Should().Contain(l =>
            l.LanguageId == updateRequest.LanguageId && l.ProficiencyLevel == updateRequest.ProficiencyLevel);
    }

    [Fact]
    public async Task ShouldNotUpdateUserLanguageWithInvalidProficiencyLevel()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var request = new UpdateUserLanguageVM
        {
            LanguageId = _language1.Id,
            ProficiencyLevel = (ProficiencyLevel)int.MaxValue // Invalid proficiency level
        };

        // Act
        var response = await Client.PostAsJsonAsync("User/languages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateUserLanguageWithInvalidLanguageId()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var request = new UpdateUserLanguageVM
        {
            LanguageId = int.MaxValue, // Non-existent language ID
            ProficiencyLevel = ProficiencyLevel.Advanced
        };

        // Act
        var response = await Client.PostAsJsonAsync("User/languages", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldDeleteUserLanguage()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Arrange
        var userLanguage = new UserLanguage
        {
            UserId = _testUser.Id,
            LanguageId = _language1.Id,
            ProficiencyLevel = ProficiencyLevel.Advanced
        };
        await Context.AddAsync(userLanguage);
        await SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"User/languages/{_language1.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var userFromDb = await Context.Set<User>()
            .Include(f => f.Languages)
            .FirstOrDefaultAsync(x => x.Id == _testUser.Id);

        userFromDb.Should().NotBeNull();
        userFromDb.Languages.Should().NotContain(l => l.LanguageId == _language1.Id);
    }

    [Fact]
    public async Task ShouldNotDeleteUserLanguageWithInvalidLanguageId()
    {
        SwitchUser(role: _testUser.Role!.Name, userId: _testUser.Id);

        // Act
        var response = await Client.DeleteAsync($"User/languages/{int.MaxValue}"); // Non-existent language ID

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldCreateUsersWithDifferentRoles()
    {
        // Arrange
        SwitchUser(role: Settings.Roles.AdminRole, userId: _adminUser.Id);

        var employerRequest = new CreateUserByAdminVM
        {
            Email = "employer@test.com",
            Password = "Test123!@#",
            RoleId = _employerRole.Id
        };

        var freelancerRequest = new CreateUserByAdminVM
        {
            Email = "freelancer@test.com",
            Password = "Test123!@#",
            RoleId = _freelancerRole.Id
        };

        var moderatorRequest = new CreateUserByAdminVM
        {
            Email = "moderator@test.com",
            Password = "Test123!@#",
            RoleId = _moderatorRole.Id
        };

        // Act
        var employerResponse = await Client.PostAsJsonAsync("User", employerRequest);
        var freelancerResponse = await Client.PostAsJsonAsync("User", freelancerRequest);
        var moderatorResponse = await Client.PostAsJsonAsync("User", moderatorRequest);

        // Assert
        employerResponse.IsSuccessStatusCode.Should().BeTrue();
        freelancerResponse.IsSuccessStatusCode.Should().BeTrue();
        moderatorResponse.IsSuccessStatusCode.Should().BeTrue();

        var employer = await JsonHelper.GetPayloadAsync<UserVM>(employerResponse);
        var freelancer = await JsonHelper.GetPayloadAsync<UserVM>(freelancerResponse);
        var moderator = await JsonHelper.GetPayloadAsync<UserVM>(moderatorResponse);

        employer.RoleId.Should().Be(_employerRole.Id);
        freelancer.RoleId.Should().Be(_freelancerRole.Id);
        moderator.RoleId.Should().Be(_moderatorRole.Id);
    }

    public async Task InitializeAsync()
    {
        _freelancerRole = GetRoleByName(Settings.Roles.FreelancerRole);
        _employerRole = GetRoleByName(Settings.Roles.EmployerRole);
        _moderatorRole = GetRoleByName(Settings.Roles.ModeratorRole);
        _adminRole = GetRoleByName(Settings.Roles.AdminRole);

        _country = CountryData.MainCountry;
        _language1 = new Language { Id = 0, Name = "English", Code = "EN" };
        _language2 = new Language { Id = 0, Name = "Spanish", Code = "ES" };

        _adminUser = UserData.CreateTestUser(
            id: UserId,
            email: "admin@test.com",
            roleId: _adminRole.Id
        );

        _testUser = UserData.CreateTestUser(
            email: "testuser@test.com",
            roleId: _employerRole.Id
        );


        await Context.AddAsync(_country);
        await Context.AddAsync(_language1);
        await Context.AddAsync(_language2);
        await Context.AddAuditableAsync(_adminUser);
        await Context.AddAuditableAsync(_testUser);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}