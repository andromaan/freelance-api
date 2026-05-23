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
using Domain.Models.Payments;
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

        var faker = new Faker(); // "en"
        var passwordHasher = new PasswordHasher();
        var defaultPasswordHash = passwordHasher.HashPassword("password123");
        var adminId = Guid.Parse(Settings.Roles.AdminId);

        // 1. Довідники
        await SeedSkillsAsync(context);
        await SeedCategoriesAsync(context);

        var roles      = await context.Roles.ToListAsync();
        var countries  = await context.Countries.ToListAsync();
        var skills     = await context.Skills.ToListAsync();
        var categories = await context.Categories.ToListAsync();

        var freelancerRole = roles.First(r => r.Name == Settings.Roles.FreelancerRole);
        var employerRole   = roles.First(r => r.Name == Settings.Roles.EmployerRole);

        // 2. Користувачі
        var freelancersUsers = await GenerateFreelancersAsync(context, faker, defaultPasswordHash, freelancerRole,
            countries, skills, adminId);
        var employerUsers = await GenerateEmployersAsync(context, faker, defaultPasswordHash, employerRole,
            countries, adminId);

        // 3. Проекти + мілстоуни
        var projects = await GenerateProjectsAsync(context, faker, employerUsers, categories);
        await GenerateMilestonesAsync(context, faker, projects);

        // 4. Заявки та контракти
        await GenerateBidsAndQuotesAsync(context, projects, freelancersUsers);
        await GenerateContractsAsync(context, projects, freelancersUsers, employerUsers);
    }

    // ── Довідники ─────────────────────────────────────────────────────────────

    private static async Task SeedSkillsAsync(AppDbContext context)
    {
        if (await context.Skills.AnyAsync()) return;

        var skills = new List<Skill>
        {
            new() { Id = 1,  Name = "C#" },
            new() { Id = 2,  Name = "Java" },
            new() { Id = 3,  Name = "Python" },
            new() { Id = 4,  Name = "JavaScript" },
            new() { Id = 5,  Name = "SQL" },
            new() { Id = 6,  Name = "AWS" },
            new() { Id = 7,  Name = "Azure" },
            new() { Id = 8,  Name = "Docker" },
            new() { Id = 9,  Name = "Kubernetes" },
            new() { Id = 10, Name = "React" }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync(c => c.Id >= 100)) return;

        var names = new[]
        {
            "Web Development", "Mobile App Development", "UI/UX Design", "Copywriting", "SEO",
            "Data Science", "Video Editing", "Backend Development", "Frontend Development", "DevOps"
        };

        context.Categories.AddRange(names.Select((name, i) => new Category { Id = 100 + i, Name = name }));
        await context.SaveChangesAsync();
    }

    // ── Користувачі ───────────────────────────────────────────────────────────

    private static async Task<List<User>> GenerateFreelancersAsync(AppDbContext context, Faker faker,
        string defaultPasswordHash, Role freelancerRole, List<Country> countries, List<Skill> skills, Guid adminId)
    {
        var freelancerFaker = new Faker<User>("en")
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Email,        f  => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _  => defaultPasswordHash)
            .RuleFor(u => u.DisplayName,  f  => f.Name.FullName())
            .RuleFor(u => u.RoleId,       _  => freelancerRole.Id)
            .RuleFor(u => u.CountryId,    f  => f.PickRandom(countries).Id)
            .RuleFor(u => u.CreatedBy,    _  => adminId)
            .RuleFor(u => u.ModifiedBy,   _  => adminId)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past().ToUniversalTime())
            .RuleFor(u => u.ModifiedAt,   f  => f.Date.Recent().ToUniversalTime());

        var testFreelancer = freelancerFaker.Clone()
            .RuleFor(u => u.Email,       _ => "freelancer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Freelancer")
            .Generate();

        var freelancersUsers = freelancerFaker.Generate(15);
        freelancersUsers.Add(testFreelancer);
        context.Users.AddRange(freelancersUsers);
        await context.SaveChangesAsync();

        context.Freelancers.AddRange(freelancersUsers.Select(u => new Freelancer
        {
            Id         = u.Id,
            Bio        = faker.Lorem.Paragraphs(2),
            Location   = faker.Address.City(),
            Skills     = faker.PickRandom(skills, faker.Random.Int(2, 5)).ToList(),
            CreatedBy  = u.Id,
            CreatedAt  = u.CreatedAt,
            ModifiedBy = u.Id,
            ModifiedAt = u.ModifiedAt
        }));

        context.UserWallets.AddRange(freelancersUsers.Select(u => new UserWallet
        {
            Id         = Guid.NewGuid(),
            Balance    = faker.Random.Decimal(100, 1000),
            Currency   = "USD",
            CreatedBy  = u.Id,
            CreatedAt  = u.CreatedAt,
            ModifiedBy = u.Id,
            ModifiedAt = u.ModifiedAt
        }));

        await context.SaveChangesAsync();
        return freelancersUsers;
    }

    private static async Task<List<User>> GenerateEmployersAsync(AppDbContext context, Faker faker,
        string defaultPasswordHash, Role employerRole, List<Country> countries, Guid adminId)
    {
        var employerFaker = new Faker<User>("en")
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Email,        f  => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _  => defaultPasswordHash)
            .RuleFor(u => u.DisplayName,  f  => f.Name.FullName())
            .RuleFor(u => u.RoleId,       _  => employerRole.Id)
            .RuleFor(u => u.CountryId,    f  => f.PickRandom(countries).Id)
            .RuleFor(u => u.CreatedBy,    _  => adminId)
            .RuleFor(u => u.ModifiedBy,   _  => adminId)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past().ToUniversalTime())
            .RuleFor(u => u.ModifiedAt,   f  => f.Date.Recent().ToUniversalTime());

        var testEmployer = employerFaker.Clone()
            .RuleFor(u => u.Email,       _ => "employer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Employer")
            .Generate();

        var employerUsers = employerFaker.Generate(10);
        employerUsers.Add(testEmployer);
        context.Users.AddRange(employerUsers);
        await context.SaveChangesAsync();

        context.Employers.AddRange(employerUsers.Select(u => new Employer
        {
            Id              = u.Id,
            CompanyName     = faker.Company.CompanyName(),
            CompanyWebsite  = faker.Internet.Url(),
            CreatedBy       = u.Id,
            CreatedAt       = u.CreatedAt,
            ModifiedBy      = u.Id,
            ModifiedAt      = u.ModifiedAt
        }));

        context.UserWallets.AddRange(employerUsers.Select(u => new UserWallet
        {
            Id         = Guid.NewGuid(),
            Balance    = faker.Random.Decimal(5000, 50000),
            Currency   = "USD",
            CreatedBy  = u.Id,
            CreatedAt  = u.CreatedAt,
            ModifiedBy = u.Id,
            ModifiedAt = u.ModifiedAt
        }));

        await context.SaveChangesAsync();
        return employerUsers;
    }

    // ── Проекти ───────────────────────────────────────────────────────────────

    private static async Task<List<Project>> GenerateProjectsAsync(AppDbContext context, Faker faker,
        List<User> employerUsers, List<Category> categories)
    {
        var projectStatuses = new[] { ProjectStatus.Open, ProjectStatus.InProgress, ProjectStatus.Completed };

        var projectFaker = new Faker<Project>("en")
            .RuleFor(p => p.Id,          _  => Guid.NewGuid())
            .RuleFor(p => p.Title,       f  => f.Lorem.Sentence(f.Random.Int(3, 7)))
            .RuleFor(p => p.Description, f  => f.Lorem.Paragraphs(f.Random.Int(1, 3)))
            .RuleFor(p => p.Budget,      f  => Math.Round(f.Random.Decimal(100, 10000), 2))
            .RuleFor(p => p.Status,      f  => f.PickRandom(projectStatuses))
            .RuleFor(p => p.Deadline,    f  => f.Date.Future().ToUniversalTime())
            .RuleFor(p => p.CreatedBy,   f  => f.PickRandom(employerUsers).Id)
            .RuleFor(p => p.CreatedAt,   f  => f.Date.Past().ToUniversalTime())
            .RuleFor(p => p.ModifiedBy, (_, p) => p.CreatedBy)
            .RuleFor(p => p.ModifiedAt,  f  => f.Date.Recent().ToUniversalTime());

        // 60 проектів замість 30
        var projects = projectFaker.Generate(60);

        foreach (var project in projects)
            project.Categories = faker.PickRandom(categories, faker.Random.Int(1, 3)).ToList();

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        return projects;
    }

    // ── Мілстоуни ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Генерує 2–4 мілстоуни для кожного проекту.
    /// Дедлайни мілстоунів рівномірно розподілені між датою створення проекту
    /// та його дедлайном, щоб хронологія виглядала реалістично.
    /// Сума Amount усіх мілстоунів не перевищує Budget проекту.
    /// </summary>
    private static async Task GenerateMilestonesAsync(AppDbContext context, Faker faker,
        List<Project> projects)
    {
        // Описові назви для мілстоунів — робить дані читабельними
        var milestoneDescriptions = new[]
        {
            "Kickoff & Requirements gathering",
            "UI/UX wireframes and design approval",
            "Backend architecture & database schema",
            "Core feature implementation",
            "API integration and testing",
            "Frontend development",
            "QA & bug fixes",
            "Performance optimization",
            "User acceptance testing (UAT)",
            "Deployment and final delivery"
        };

        var milestones = new List<ProjectMilestone>();

        foreach (var project in projects)
        {
            var count = faker.Random.Int(2, 4);

            // Розбиваємо бюджет на частини (рандомно, але в сумі ≤ budget)
            var shares = Enumerable.Range(0, count)
                .Select(_ => faker.Random.Double(0.1))
                .ToList();
            var total = shares.Sum();
            var amounts = shares
                .Select(s => Math.Round((decimal)(s / total) * project.Budget * faker.Random.Decimal(0.7m, 0.95m), 2))
                .ToList();

            // Рівномірно ділимо часовий проміжок між CreatedAt та Deadline
            var start   = project.CreatedAt;
            var end     = project.Deadline;
            var span    = (end - start).TotalSeconds / (count + 1);

            var descriptions = faker.PickRandom(milestoneDescriptions, count).ToList();

            for (var i = 0; i < count; i++)
            {
                milestones.Add(new ProjectMilestone
                {
                    Id          = Guid.NewGuid(),
                    ProjectId   = project.Id,
                    Description = descriptions[i],
                    Amount      = amounts[i],
                    DueDate     = start.AddSeconds(span * (i + 1)).ToUniversalTime(),
                    CreatedBy   = project.CreatedBy,
                    CreatedAt   = project.CreatedAt,
                    ModifiedBy  = project.ModifiedBy,
                    ModifiedAt  = project.ModifiedAt
                });
            }
        }

        context.ProjectMilestones.AddRange(milestones);
        await context.SaveChangesAsync();
    }

    // ── Заявки та контракти ───────────────────────────────────────────────────

    private static async Task GenerateBidsAndQuotesAsync(AppDbContext context, List<Project> projects,
        List<User> freelancersUsers)
    {
        var openProjects = projects.Where(p => p.Status == ProjectStatus.Open).ToList();
        if (!openProjects.Any()) return;

        var bidFaker = new Faker<Bid>("en")
            .RuleFor(b => b.Id,           _  => Guid.NewGuid())
            .RuleFor(b => b.ProjectId,    f  => f.PickRandom(openProjects).Id)
            .RuleFor(b => b.Amount,       (f, b) =>
                Math.Round(f.Random.Decimal(100, openProjects.First(x => x.Id == b.ProjectId).Budget), 2))
            .RuleFor(b => b.Message,      f  => f.Lorem.Paragraph())
            .RuleFor(b => b.FreelancerId, f  => f.PickRandom(freelancersUsers).Id)
            .RuleFor(b => b.CreatedBy,    (_, b) => b.FreelancerId)
            .RuleFor(b => b.CreatedAt,    f  => f.Date.Recent().ToUniversalTime())
            .RuleFor(b => b.ModifiedBy,   (_, b) => b.FreelancerId)
            .RuleFor(b => b.ModifiedAt,   (_, b) => b.CreatedAt);

        context.Bids.AddRange(bidFaker.Generate(80)); // більше заявок під більше проектів
        await context.SaveChangesAsync();

        var quoteFaker = new Faker<Quote>("en")
            .RuleFor(q => q.Id,           _  => Guid.NewGuid())
            .RuleFor(q => q.ProjectId,    f  => f.PickRandom(openProjects).Id)
            .RuleFor(q => q.Amount,       (f, q) =>
                Math.Round(f.Random.Decimal(100, openProjects.First(x => x.Id == q.ProjectId).Budget), 2))
            .RuleFor(q => q.Message,      f  => f.Lorem.Paragraph())
            .RuleFor(q => q.FreelancerId, f  => f.PickRandom(freelancersUsers).Id)
            .RuleFor(q => q.CreatedBy,    (_, q) => q.FreelancerId)
            .RuleFor(q => q.CreatedAt,    f  => f.Date.Recent().ToUniversalTime())
            .RuleFor(q => q.ModifiedBy,   (_, q) => q.FreelancerId)
            .RuleFor(q => q.ModifiedAt,   (_, q) => q.CreatedAt);

        context.Quotes.AddRange(quoteFaker.Generate(50));
        await context.SaveChangesAsync();
    }

    private static async Task GenerateContractsAsync(AppDbContext context, List<Project> projects,
        List<User> freelancersUsers, List<User> employerUsers)
    {
        var inProgressProjects = projects
            .Where(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Completed).ToList();

        if (!inProgressProjects.Any()) return;

        var statuses = new[] { ContractStatus.Active, ContractStatus.Completed, ContractStatus.Pending };

        var contractFaker = new Faker<Contract>("en")
            .RuleFor(c => c.Id,           _  => Guid.NewGuid())
            .RuleFor(c => c.ProjectId,    f  => f.PickRandom(inProgressProjects).Id)
            .RuleFor(c => c.FreelancerId, f  => f.PickRandom(freelancersUsers).Id)
            .RuleFor(c => c.StartDate,    f  => f.Date.Past().ToUniversalTime())
            .RuleFor(c => c.AgreedRate,   (f, c) =>
                Math.Round(f.Random.Decimal(100, inProgressProjects.First(x => x.Id == c.ProjectId).Budget), 2))
            .RuleFor(c => c.Status,       f  => f.PickRandom(statuses))
            .RuleFor(c => c.CreatedBy,    f  => f.PickRandom(employerUsers).Id)
            .RuleFor(c => c.CreatedAt,    (_, c) => c.StartDate)
            .RuleFor(c => c.ModifiedBy,   (_, c) => c.CreatedBy)
            .RuleFor(c => c.ModifiedAt,   (_, c) => c.StartDate);

        context.Contracts.AddRange(contractFaker.Generate(inProgressProjects.Count * 2));
        await context.SaveChangesAsync();
    }
}