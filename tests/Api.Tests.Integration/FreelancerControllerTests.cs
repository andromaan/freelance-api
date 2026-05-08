using System.Net.Http.Json;
using BLL.ViewModels.Freelancer;
using DAL.Extensions;
using Domain.Models.Countries;
using Domain.Models.Freelance;
using Domain.Models.Languages;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class FreelancerControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private Freelancer _freelancer = null!;
    private User _user = null!;
    
    private Skill _skill1 = null!;
    private Skill _skill2 = null!;

    [Fact]
    public async Task ShouldGetFreelancerByUser()
    {
        // Act
        var response = await Client.GetAsync("Freelancer");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var freelancerFromResponse = await JsonHelper.GetPayloadAsync<FreelancerVM>(response);
        
        freelancerFromResponse.Should().NotBeNull();
        freelancerFromResponse.Bio.Should().Be(_freelancer.Bio);
        freelancerFromResponse.Location.Should().Be(_freelancer.Location);
    }
    
    [Fact]
    public async Task ShouldUpdateFreelancer()
    {
        // Arrange
        var request = new UpdateFreelancerVM 
        { 
            Bio = "Updated Bio",
            Location = "Updated Location"
        };

        // Act
        var response = await Client.PutAsJsonAsync("Freelancer", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        // var freelancerFromResponse = await JsonHelper.GetPayloadAsync<FreelancerVM>(response);
        
        var freelancerFromDb = await Context.Freelancers
            .FirstOrDefaultAsync(x => x.CreatedBy == _user.Id);
        
        freelancerFromDb.Should().NotBeNull();
        freelancerFromDb.Bio.Should().Be("Updated Bio");
        freelancerFromDb.Location.Should().Be("Updated Location");
    }
    
    [Fact]
    public async Task ShouldUpdateFreelancerSkills()
    {
        // Arrange
        var request = new UpdateFreelancerSkillsVM 
        { 
            SkillIds = new List<int> { _skill1.Id, _skill2.Id }
        };

        // Act
        var response = await Client.PutAsJsonAsync("Freelancer/skills", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var freelancerFromDb = await Context.Freelancers
            .Include(f => f.Skills)
            .FirstOrDefaultAsync(x => x.CreatedBy == _user.Id);
        
        freelancerFromDb.Should().NotBeNull();
        freelancerFromDb.Skills.Should().HaveCount(2);
        freelancerFromDb.Skills.Should().Contain(l => l.Id == _skill1.Id);
        freelancerFromDb.Skills.Should().Contain(l => l.Id == _skill2.Id);
    }
    
    [Fact]
    public async Task ShouldUpdateFreelancerSkillsWithEmptyList()
    {
        // Arrange
        var request = new UpdateFreelancerSkillsVM 
        { 
            SkillIds = new List<int>()
        };

        // Act
        var response = await Client.PutAsJsonAsync("Freelancer/skills", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var freelancerFromDb = await Context.Freelancers
            .Include(f => f.Skills)
            .FirstOrDefaultAsync(x => x.CreatedBy == _user.Id);
        
        freelancerFromDb.Should().NotBeNull();
        freelancerFromDb.Skills.Should().BeEmpty();
    }

    public async Task InitializeAsync()
    {
        _user = UserData.CreateTestUser(UserId);
        
        _skill1 = new Skill { Id = 0, Name = "C#" };
        _skill2 = new Skill { Id = 0, Name = "React" };
        
        _freelancer = FreelancerData.CreateFreelancer(userId: _user.Id);

        await Context.AddAuditableAsync(_user);
        await Context.AddAsync(_skill1);
        await Context.AddAsync(_skill2);
        await Context.AddAuditableAsync(_freelancer);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}
