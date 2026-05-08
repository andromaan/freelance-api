using BLL;
using BLL.Services.PasswordHasher;
using Bogus;
using Domain.Models.Auth;
using Domain.Models.Contracts;
using Domain.Models.Countries;
using Domain.Models.Employers;
using Domain.Models.Freelance;
using Domain.Models.Projects;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.Initializer;

public static class BogusDataSeeder
{
    public static async Task SeedTestDataAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "freelancer@test.com"))
        {
            return; // Already seeded
        }

        var faker = new Faker("uk");
        var passwordHasher = new PasswordHasher();
        var defaultPasswordHash = passwordHasher.HashPassword("TestPass123!");
        var adminId = Guid.Parse(Settings.Roles.AdminId); // ID користувача Admin

        // 1. Створення необхідних довідників
        await SeedSkillsAsync(context);
        await SeedCategoriesAsync(context);

        // Retrieve existing lookup data
        var roles = await context.Roles.ToListAsync();
        var countries = await context.Countries.ToListAsync();
        var skills = await context.Skills.ToListAsync();
        var categories = await context.Categories.ToListAsync();

        var freelancerRole = roles.First(r => r.Name == Settings.Roles.FreelancerRole);
        var employerRole = roles.First(r => r.Name == Settings.Roles.EmployerRole);

        // 2. Генерація користувачів
        var freelancersUsers = await GenerateFreelancersAsync(context, faker, defaultPasswordHash, freelancerRole,
            countries, skills, adminId);
        var employerUsers =
            await GenerateEmployersAsync(context, faker, defaultPasswordHash, employerRole, countries, adminId);

        // 3. Генерація проектів
        var projects = await GenerateProjectsAsync(context, faker, employerUsers, categories);

        // 4. Генерація заявок та контрактів
        await GenerateBidsAndQuotesAsync(context, projects, freelancersUsers);
        await GenerateContractsAsync(context, projects, freelancersUsers, employerUsers);
    }

    private static async Task SeedSkillsAsync(AppDbContext context)
    {
        var existingSkills = await context.Skills.ToListAsync();
        if (existingSkills.Any()) return;

        var skills = new List<Skill>
        {
            new() { Id = 1, Name = "C#" },
            new() { Id = 2, Name = "Java" },
            new() { Id = 3, Name = "Python" },
            new() { Id = 4, Name = "JavaScript" },
            new() { Id = 5, Name = "SQL" },
            new() { Id = 6, Name = "AWS" },
            new() { Id = 7, Name = "Azure" },
            new() { Id = 8, Name = "Docker" },
            new() { Id = 9, Name = "Kubernetes" },
            new() { Id = 10, Name = "React" }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var categoriesList = new List<string>
        {
            "Web Development", "Mobile App Development", "UI/UX Design", "Copywriting", "SEO",
            "Data Science", "Video Editing", "Backend Development", "Frontend Development", "DevOps"
        };

        var categories = await context.Categories.ToListAsync();
        if (!categories.Any(c => c.Id >= 100))
        {
            var newCategories = categoriesList.Select((c, i) => new Category { Id = 100 + i, Name = c }).ToList();
            context.Categories.AddRange(newCategories);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<List<User>> GenerateFreelancersAsync(AppDbContext context, Faker faker,
        string defaultPasswordHash, Role freelancerRole, List<Country> countries, List<Skill> skills, Guid adminId)
    {
        var freelancerFaker = new Faker<User>("uk")
            .RuleFor(u => u.Id, _ => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _ => defaultPasswordHash)
            .RuleFor(u => u.DisplayName, f => f.Name.FullName())
            .RuleFor(u => u.RoleId, _ => freelancerRole.Id)
            .RuleFor(u => u.CountryId, f => f.PickRandom(countries).Id)
            .RuleFor(u => u.CreatedBy, _ => adminId)
            .RuleFor(u => u.ModifiedBy, _ => adminId)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(u => u.ModifiedAt, f => f.Date.Recent().ToUniversalTime());

        var testFreelancer = freelancerFaker.Clone()
            .RuleFor(u => u.Email, _ => "freelancer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Freelancer")
            .Generate();

        var freelancersUsers = freelancerFaker.Generate(15);
        freelancersUsers.Add(testFreelancer);
        context.Users.AddRange(freelancersUsers);
        await context.SaveChangesAsync();

        var freelancerProfiles = freelancersUsers.Select(u => new Freelancer
        {
            Id = u.Id,
            Bio = faker.Lorem.Paragraphs(2),
            Location = faker.Address.City(),
            Skills = faker.PickRandom(skills, faker.Random.Int(2, 5)).ToList(),
            CreatedBy = adminId,
            CreatedAt = u.CreatedAt,
            ModifiedBy = adminId,
            ModifiedAt = u.ModifiedAt
        }).ToList();
        context.Freelancers.AddRange(freelancerProfiles);
        await context.SaveChangesAsync();

        return freelancersUsers;
    }

    private static async Task<List<User>> GenerateEmployersAsync(AppDbContext context, Faker faker,
        string defaultPasswordHash, Role employerRole, List<Country> countries, Guid adminId)
    {
        var employerFaker = new Faker<User>("uk")
            .RuleFor(u => u.Id, _ => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _ => defaultPasswordHash)
            .RuleFor(u => u.DisplayName, f => f.Name.FullName())
            .RuleFor(u => u.RoleId, _ => employerRole.Id)
            .RuleFor(u => u.CountryId, f => f.PickRandom(countries).Id)
            .RuleFor(u => u.CreatedBy, _ => adminId)
            .RuleFor(u => u.ModifiedBy, _ => adminId)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(u => u.ModifiedAt, f => f.Date.Recent().ToUniversalTime());

        var testEmployer = employerFaker.Clone()
            .RuleFor(u => u.Email, _ => "employer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Employer")
            .Generate();

        var employerUsers = employerFaker.Generate(10);
        employerUsers.Add(testEmployer);
        context.Users.AddRange(employerUsers);
        await context.SaveChangesAsync();

        var employerProfiles = employerUsers.Select(u => new Employer
        {
            Id = u.Id,
            CompanyName = faker.Company.CompanyName(),
            CompanyWebsite = faker.Internet.Url(),
            CreatedBy = adminId,
            CreatedAt = u.CreatedAt,
            ModifiedBy = adminId,
            ModifiedAt = u.ModifiedAt
        }).ToList();
        context.Employers.AddRange(employerProfiles);
        await context.SaveChangesAsync();

        return employerUsers;
    }

    private static async Task<List<Project>> GenerateProjectsAsync(AppDbContext context, Faker faker,
        List<User> employerUsers, List<Category> categories)
    {
        var projectStatuses = new[] { ProjectStatus.Open, ProjectStatus.InProgress, ProjectStatus.Completed };

        var projectFaker = new Faker<Project>("uk")
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Title, f => f.Lorem.Sentence(f.Random.Int(3, 7)))
            .RuleFor(p => p.Description, f => f.Lorem.Paragraphs(f.Random.Int(1, 3)))
            .RuleFor(p => p.Budget, f => Math.Round(f.Random.Decimal(100, 10000), 2))
            .RuleFor(p => p.Status, f => f.PickRandom(projectStatuses))
            .RuleFor(p => p.Deadline, f => f.Date.Future().ToUniversalTime())
            .RuleFor(p => p.CreatedBy, f => f.PickRandom(employerUsers).Id)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(p => p.ModifiedBy, f => f.PickRandom(employerUsers).Id)
            .RuleFor(p => p.ModifiedAt, f => f.Date.Recent().ToUniversalTime());

        var projects = projectFaker.Generate(30);

        foreach (var project in projects)
        {
            project.Categories = faker.PickRandom(categories, faker.Random.Int(1, 3)).ToList();
        }

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        return projects;
    }

    private static async Task GenerateBidsAndQuotesAsync(AppDbContext context, List<Project> projects,
        List<User> freelancersUsers)
    {
        var openProjects = projects.Where(p => p.Status == ProjectStatus.Open).ToList();

        if (!openProjects.Any()) return;

        var bidFaker = new Faker<Bid>("uk")
            .RuleFor(b => b.Id, _ => Guid.NewGuid())
            .RuleFor(b => b.ProjectId, f => f.PickRandom(openProjects).Id)
            .RuleFor(b => b.Amount,
                (f, b) => Math.Round(f.Random.Decimal(100, openProjects.First(x => x.Id == b.ProjectId).Budget), 2))
            .RuleFor(b => b.Message, f => f.Lorem.Paragraph())
            .RuleFor(b => b.FreelancerId, f => f.PickRandom(freelancersUsers).Id)
            .RuleFor(b => b.CreatedBy, (_, b) => b.FreelancerId)
            .RuleFor(b => b.CreatedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(b => b.ModifiedBy, (_, b) => b.FreelancerId)
            .RuleFor(b => b.ModifiedAt, (_, b) => b.CreatedAt);

        var bids = bidFaker.Generate(50);
        context.Bids.AddRange(bids);
        await context.SaveChangesAsync();

        var quoteFaker = new Faker<Quote>("uk")
            .RuleFor(q => q.Id, _ => Guid.NewGuid())
            .RuleFor(q => q.ProjectId, f => f.PickRandom(openProjects).Id)
            .RuleFor(q => q.Amount,
                (f, q) => Math.Round(f.Random.Decimal(100, openProjects.First(x => x.Id == q.ProjectId).Budget), 2))
            .RuleFor(q => q.Message, f => f.Lorem.Paragraph())
            .RuleFor(q => q.FreelancerId, f => f.PickRandom(freelancersUsers).Id)
            .RuleFor(q => q.CreatedBy, (_, q) => q.FreelancerId)
            .RuleFor(q => q.CreatedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(q => q.ModifiedBy, (_, q) => q.FreelancerId)
            .RuleFor(q => q.ModifiedAt, (_, q) => q.CreatedAt);

        var quotes = quoteFaker.Generate(30);
        context.Quotes.AddRange(quotes);
        await context.SaveChangesAsync();
    }

    private static async Task GenerateContractsAsync(AppDbContext context, List<Project> projects,
        List<User> freelancersUsers, List<User> employerUsers)
    {
        var inProgressProjects = projects
            .Where(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Completed).ToList();

        if (!inProgressProjects.Any()) return;

        var statuses = new[] { ContractStatus.Active, ContractStatus.Completed, ContractStatus.Pending };
        var contractFaker = new Faker<Contract>("uk")
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.ProjectId, f => f.PickRandom(inProgressProjects).Id)
            .RuleFor(c => c.FreelancerId, f => f.PickRandom(freelancersUsers).Id)
            .RuleFor(c => c.StartDate, f => f.Date.Past().ToUniversalTime())
            .RuleFor(c => c.AgreedRate,
                (f, c) => Math.Round(f.Random.Decimal(100, inProgressProjects.First(x => x.Id == c.ProjectId).Budget),
                    2))
            .RuleFor(c => c.Status, f => f.PickRandom(statuses))
            .RuleFor(c => c.CreatedBy, f => f.PickRandom(employerUsers).Id)
            .RuleFor(c => c.CreatedAt, (_, c) => c.StartDate)
            .RuleFor(c => c.ModifiedBy, (_, c) => c.CreatedBy)
            .RuleFor(c => c.ModifiedAt, (_, c) => c.StartDate);

        var contracts = contractFaker.Generate(inProgressProjects.Count * 2);
        context.Contracts.AddRange(contracts);
        await context.SaveChangesAsync();
    }
}