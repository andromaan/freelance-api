using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using BLL;
using DAL.Data;
using Domain.Models.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    private const string JwtIssuer = "oa.edu.ua";
    private const string JwtAudience = "oa.edu.ua";
    private const string JwtSecretKey = "1fd8bcc13347efbdebb5d7660e22ffb346f8104eeb925ef0eca6b85ddbd4edbf";
    protected readonly AppDbContext Context;
    protected readonly HttpClient Client;
    protected Guid UserId { get; private set; } = Guid.NewGuid();

    protected BaseIntegrationTest(IntegrationTestWebFactory factory, bool useJwtToken = true, string? customRole = null)
    {
        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        if (useJwtToken)
            SetAuthorizationHeader(customRole);
    }

    // Новий метод для зміни ролі та userId
    protected void SwitchUser(string role, Guid? userId = null)
    {
        if (userId.HasValue)
        {
            UserId = userId.Value;
        }

        SetAuthorizationHeader(role);
    }

    private void SetAuthorizationHeader(string? customRole = null)
    {
        var token = GenerateJwtToken(customRole);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    private string GenerateJwtToken(string? customRole = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var role = customRole ?? Settings.Roles.AdminRole;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new("id", UserId.ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddYears(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected async Task ClearDatabaseAsync()
    {
        var tableNames = Context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => t != null && t != "roles")
            .Distinct()
            .ToList();

        var tableList = string.Join(", ", tableNames.Select(t => $"\"{t}\""));
        
        #pragma warning disable EF1002
        await Context.Database.ExecuteSqlRawAsync(
            $"TRUNCATE {tableList} RESTART IDENTITY CASCADE");
    }

    protected int GetRoleIdByName(string? role)
        => GetRoleByName(role).Id;

    protected Role GetRoleByName(string? role)
        => Context.Roles.First(r => r.Name == role);
}