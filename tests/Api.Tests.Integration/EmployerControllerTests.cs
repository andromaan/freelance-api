using System.Net;
using System.Net.Http.Json;
using BLL.ViewModels.Employer;
using DAL.Extensions;
using Domain.Models.Employers;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class EmployerControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private Employer _employer = null!;
    private User _user = null!;

    [Fact]
    public async Task ShouldGetEmployerByUser()
    {
        // Act
        var response = await Client.GetAsync("Employer");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var employerFromResponse = await JsonHelper.GetPayloadAsync<EmployerVM>(response);
        
        employerFromResponse.Should().NotBeNull();
        employerFromResponse.Id.Should().Be(_employer.Id);
        employerFromResponse.CreatedBy.Should().Be(_employer.CreatedBy);
        employerFromResponse.CompanyName.Should().Be(_employer.CompanyName);
    }
    
    [Fact]
    public async Task ShouldUpdateEmployer()
    {
        // Arrange
        var request = new UpdateEmployerVM 
        { 
            CompanyName = "Updated Company",
            CompanyWebsite = "https://updatedcompany.com"
        };

        // Act
        var response = await Client.PutAsJsonAsync("Employer", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var employerFromResponse = await JsonHelper.GetPayloadAsync<EmployerVM>(response);
        
        var employerFromDb = await Context.Employers.FirstOrDefaultAsync(x => x.Id == employerFromResponse.Id);
        
        employerFromDb.Should().NotBeNull();
        employerFromDb.CompanyName.Should().Be("Updated Company");
        employerFromDb.CompanyWebsite.Should().Be("https://updatedcompany.com");
    }
    
    [Fact]
    public async Task ShouldValidateCompanyNameIsRequired()
    {
        // Arrange
        var request = new UpdateEmployerVM 
        { 
            CompanyName = "",
            CompanyWebsite = "https://test.com"
        };

        // Act
        var response = await Client.PutAsJsonAsync("Employer", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public async Task InitializeAsync()
    {
        _user = UserData.CreateTestUser(UserId);
        _employer = EmployerData.CreateEmployer(createdBy: _user.Id);

        await Context.AddAuditableAsync(_user);
        await Context.AddAuditableAsync(_employer);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}
