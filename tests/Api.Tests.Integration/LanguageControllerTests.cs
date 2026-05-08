using System.Net;
using System.Net.Http.Json;
using BLL.ViewModels.Language;
using Domain.Models.Languages;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class LanguageControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private readonly Language _language = LanguageData.MainLanguage;

    [Fact]
    public async Task ShouldCreateLanguage()
    {
        // Arrange
        var languageName = "TestLanguage";
        var languageCode = "AA";
        var request = new CreateLanguageVM { Name = languageName, Code = languageCode };

        // Act
        var response = await Client.PostAsJsonAsync("Language", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var languageFromResponse = await JsonHelper.GetPayloadAsync<LanguageVM>(response);
        var languageId = languageFromResponse.Id;

        var languageFromDb = await Context.Languages.FirstOrDefaultAsync(x => x.Id == languageId);

        languageFromDb.Should().NotBeNull();
        languageFromDb.Name.Should().Be(languageName);
        languageFromDb.Code.Should().Be(languageCode);
    }
    
    [Fact]
    public async Task ShouldUpdateLanguage()
    {
        // Arrange
        var languageName = "TestLanguage";
        var languageCode = "AA";
        var request = new UpdateLanguageVM { Name = languageName, Code = languageCode };

        // Act
        var response = await Client.PutAsJsonAsync($"Language/{_language.Id}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var languageFromResponse = await JsonHelper.GetPayloadAsync<LanguageVM>(response);
        var languageId = languageFromResponse.Id;
        
        var languageFromDb = await Context.Languages.FirstOrDefaultAsync(x => x.Id == languageId);
        
        languageFromDb.Should().NotBeNull();
        languageFromDb.Name.Should().Be(languageName);
        languageFromDb.Code.Should().Be(languageCode);
    }
    
    [Fact]
    public async Task ShouldDeleteLanguage()
    {
        // Act
        var response = await Client.DeleteAsync($"Language/{_language.Id}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var languageFromDb = await Context.Languages.FirstOrDefaultAsync(x => x.Id == _language.Id);
        
        languageFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotUpdateBecauseNotFound()
    {
        // Arrange
        var request = new UpdateLanguageVM { Name = "TestLanguage", Code = "AA" };
        
        // Act
        var response = await Client.PutAsJsonAsync($"Language/{int.MaxValue}", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotDeleteBecauseNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"Language/{int.MaxValue}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotGetByIdBecauseNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Language/{int.MaxValue}");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task ShouldNotCreateBecauseDuplicateCode()
    {
        // Arrange
        var request = new CreateLanguageVM { Name = "TestLanguage", Code = _language.Code };
        
        // Act
        var response = await Client.PostAsJsonAsync("Language", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldNotCreateBecauseDuplicateName()
    {
        // Arrange
        var request = new CreateLanguageVM { Name = _language.Name, Code = "TM" };
        
        // Act
        var response = await Client.PostAsJsonAsync("Language", request);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ShouldGetAllLanguages()
    {
        // Act
        var response = await Client.GetAsync("Language");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var languages = await JsonHelper.GetPayloadAsync<List<LanguageVM>>(response);
        
        languages.Should().NotBeEmpty();
    }

    public async Task InitializeAsync()
    {
        await Context.AddAsync(_language);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Languages.RemoveRange(Context.Languages);
        await SaveChangesAsync();
    }
}