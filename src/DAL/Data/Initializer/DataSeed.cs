using System.Text.Json;
using BLL;
using BLL.Services.PasswordHasher;
using Domain.Models.Auth;
using Domain.Models.Countries;
using Domain.Models.Languages;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.Initializer;

public static partial class DataSeed
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        var adminRole = SeedRoles(modelBuilder);
        SeedLanguages(modelBuilder);
        SeedCountries(modelBuilder);
        SeedUsers(modelBuilder, adminRole);
    }

    private static void SeedUsers(ModelBuilder modelBuilder, Role adminRole)
    {
        var passwordHasher = new PasswordHasher();

        var adminId = Guid.Parse(Settings.Roles.AdminId);

        var adminUser = new User
        {
            Id = Guid.Parse(Settings.Roles.AdminId),
            DisplayName = "Admin",
            PasswordHash = passwordHasher.HashPassword("admin"),
            Email = "admin@mail.com",
            RoleId = adminRole.Id,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow,
            ModifiedBy = adminId,
            ModifiedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<User>().HasData(adminUser);
    }

    private static Role SeedRoles(ModelBuilder modelBuilder)
    {
        var roles = new List<Role>();

        foreach (var role in Settings.Roles.ListOfRoles)
        {
            roles.Add(new Role { Name = role, Id = roles.Count + 1 });
        }

        modelBuilder.Entity<Role>().HasData(roles);
        
        return roles.FirstOrDefault(r => r.Name == Settings.Roles.AdminRole)!;
    }

    private static void SeedLanguages(ModelBuilder modelBuilder)
    {
        try
        {
            var json = File.ReadAllText("wwwroot/languages/languages.json");
            var languagesDto = JsonSerializer.Deserialize<List<LanguageDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var languages = languagesDto!
                .Select((l, index) => new { Id = index + 1, l.Code, l.Name });

            modelBuilder.Entity<Language>().HasData(languages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding languages: {ex.Message}");
        }
    }


    private static void SeedCountries(ModelBuilder modelBuilder)
    {
        try
        {
            var json = File.ReadAllText("wwwroot/countries/countries.json");
            var countryDtos = JsonSerializer.Deserialize<List<CountryDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (countryDtos == null || !countryDtos.Any())
            {
                Console.WriteLine("Warning: No countries found in the JSON file or the file is empty.");
                return;
            }

            var countries = countryDtos
                .Where(c => !string.IsNullOrWhiteSpace(c.Alpha2) && !string.IsNullOrWhiteSpace(c.Name) &&
                            !string.IsNullOrWhiteSpace(c.Alpha3))
                .Select((c, index) => new Country
                {
                    Id = index + 1,
                    Name = c.Name.Trim(),
                    Alpha2Code = c.Alpha2.Trim().ToUpper(),
                    Alpha3Code = c.Alpha3.Trim().ToUpper()
                })
                .ToList();

            modelBuilder.Entity<Country>().HasData(countries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding countries: {ex.Message}");
        }
    }
}