using System.Net;
using System.Net.Http.Json;
using BLL.ViewModels.Skill;
using Domain.Models.Projects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class SkillControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private readonly Skill _skill = SkillData.MainSkill;

    [Fact]
    public async Task ShouldCreateSkill()
    {
        // Arrange
        var skillName = "TestSkill";
        var request = new CreateSkillVM { Name = skillName };

        // Act
        var response = await Client.PostAsJsonAsync("Skill", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var skillFromResponse = await JsonHelper.GetPayloadAsync<SkillVM>(response);
        var skillId = skillFromResponse.Id;

        var skillFromDb = await Context.Set<Skill>().FirstOrDefaultAsync(x => x.Id == skillId);

        skillFromDb.Should().NotBeNull();
        skillFromDb.Name.Should().Be(skillName);
    }
    
    [Fact]
    public async Task ShouldUpdateSkill()
    {
        // Arrange
        var skillName = "UpdatedSkill";
        var request = new UpdateSkillVM { Name = skillName };

        // Act
        var response = await Client.PutAsJsonAsync($"Skill/{_skill.Id}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var skillFromResponse = await JsonHelper.GetPayloadAsync<SkillVM>(response);
        var skillId = skillFromResponse.Id;
        
        var skillFromDb = await Context.Set<Skill>().FirstOrDefaultAsync(x => x.Id == skillId);
        
        skillFromDb.Should().NotBeNull();
        skillFromDb.Name.Should().Be(skillName);
    }
    
    [Fact]
    public async Task ShouldDeleteSkill()
    {
        // Act
        var response = await Client.DeleteAsync($"Skill/{_skill.Id}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var skillFromDb = await Context.Set<Skill>().FirstOrDefaultAsync(x => x.Id == _skill.Id);
        
        skillFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetSkillById()
    {
        // Act
        var response = await Client.GetAsync($"Skill/{_skill.Id}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var skillFromResponse = await JsonHelper.GetPayloadAsync<SkillVM>(response);
        
        skillFromResponse.Should().NotBeNull();
        skillFromResponse.Id.Should().Be(_skill.Id);
        skillFromResponse.Name.Should().Be(_skill.Name);
    }

    [Fact]
    public async Task ShouldNotUpdateBecauseNotFound()
    {
        // Arrange
        var request = new UpdateSkillVM { Name = "TestSkill" };
        
        // Act
        var response = await Client.PutAsJsonAsync($"Skill/{int.MaxValue}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotDeleteBecauseNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"Skill/{int.MaxValue}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotGetByIdBecauseNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Skill/{int.MaxValue}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldGetAllSkills()
    {
        // Act
        var response = await Client.GetAsync("Skill");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var skills = await JsonHelper.GetPayloadAsync<List<SkillVM>>(response);
        
        skills.Should().NotBeEmpty();
    }

    public async Task InitializeAsync()
    {
        await Context.AddAsync(_skill);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Set<Skill>().RemoveRange(Context.Set<Skill>());
        await SaveChangesAsync();
    }
}
