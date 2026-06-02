using BLL;
using BLL.Services.PasswordHasher;
using Bogus;
using Domain.Models.Auth;
using Domain.Models.Contracts;
using Domain.Models.Countries;
using Domain.Models.Disputes;
using Domain.Models.Employers;
using Domain.Models.Freelance;
using Domain.Models.Languages;
using Domain.Models.Messaging;
using Domain.Models.Notifications;
using Domain.Models.Payments;
using Domain.Models.Projects;
using Domain.Models.Reviews;
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

        var faker = new Faker(); // "en"
        var passwordHasher = new PasswordHasher();
        var defaultPasswordHash = passwordHasher.HashPassword("password123");
        var adminId = Guid.Parse(Settings.Roles.AdminId);

        // 1. Довідники
        await SeedSkillsAsync(context);
        await SeedCategoriesAsync(context);
        await SeedLanguagesAsync(context);

        var roles      = await context.Roles.ToListAsync();
        var countries  = await context.Countries.ToListAsync();
        var skills     = await context.Skills.ToListAsync();
        var categories = await context.Categories.ToListAsync();
        var languages  = await context.Languages.ToListAsync();

        var freelancerRole = roles.First(r => r.Name == Settings.Roles.FreelancerRole);
        var employerRole   = roles.First(r => r.Name == Settings.Roles.EmployerRole);
        var moderatorRole  = roles.First(r => r.Name == Settings.Roles.ModeratorRole);

        // Генерація Модераторів
        var moderatorUsers = await GenerateModeratorsAsync(context, defaultPasswordHash, moderatorRole, countries, adminId);

        // 2. Користувачі
        var freelancersUsers = await GenerateFreelancersAsync(context, faker, defaultPasswordHash, freelancerRole,
            countries, skills, languages, adminId);
        var employerUsers = await GenerateEmployersAsync(context, faker, defaultPasswordHash, employerRole,
            countries, languages, adminId);

        // 3. Проекти + мілстоуни
        var projects = await GenerateProjectsAsync(context, faker, employerUsers, categories);
        await GenerateProjectMilestonesAsync(context, faker, projects);

        // 4. Заявки та контракти
        await GenerateBidsAndQuotesAsync(context, projects, freelancersUsers);
        var contracts = await GenerateContractsAsync(context, faker, projects, freelancersUsers, employerUsers);

        // 5. Повідомлення
        await GenerateMessagesAsync(context, faker, contracts);

        // 6. Відгуки
        await GenerateReviewsAsync(context, faker, contracts, freelancerRole.Id, employerRole.Id);

        // 7. Платежі і Транзакції
        await GeneratePaymentsAndTransactionsAsync(context, faker, contracts);

        // 8. Спори
        await GenerateDisputesAsync(context, faker, contracts, moderatorUsers);

        // 9. Сповіщення
        await GenerateNotificationsAsync(context, faker, freelancersUsers, employerUsers);
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
            new() { Id = 10, Name = "React" },
            new() { Id = 11, Name = "Angular" },
            new() { Id = 12, Name = "Node.js" },
            new() { Id = 13, Name = "Figma" },
            new() { Id = 14, Name = "SEO Optimization" },
            new() { Id = 15, Name = "Copywriting" },
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
            "Data Science", "Video Editing", "Backend Development", "Frontend Development", "DevOps",
            "QA Testing", "Game Development", "Cybersecurity", "Blockchain", "3D Modeling"
        };

        context.Categories.AddRange(names.Select((name, i) => new Category { Id = 100 + i, Name = name }));
        await context.SaveChangesAsync();
    }

    private static async Task SeedLanguagesAsync(AppDbContext context)
    {
        if (await context.Languages.AnyAsync()) return;

        var langs = new List<Language>
        {
            new() { Id = 1, Name = "English" },
            new() { Id = 2, Name = "Ukrainian" },
            new() { Id = 3, Name = "Spanish" },
            new() { Id = 4, Name = "German" },
            new() { Id = 5, Name = "French" },
            new() { Id = 6, Name = "Polish" },
            new() { Id = 7, Name = "Italian" }
        };

        context.Languages.AddRange(langs);
        await context.SaveChangesAsync();
    }

    // ── Користувачі ───────────────────────────────────────────────────────────

    private static async Task<List<User>> GenerateModeratorsAsync(AppDbContext context,
        string defaultPasswordHash, Role moderatorRole, List<Country> countries, Guid adminId)
    {
        var modFaker = new Faker<User>("en")
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Email,        f  => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _  => defaultPasswordHash)
            .RuleFor(u => u.DisplayName,  f  => f.Name.FullName())
            .RuleFor(u => u.RoleId,       _  => moderatorRole.Id)
            .RuleFor(u => u.CountryId,    f  => f.PickRandom(countries).Id)
            .RuleFor(u => u.CreatedBy,    _  => adminId)
            .RuleFor(u => u.ModifiedBy,   _  => adminId)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past(2).ToUniversalTime())
            .RuleFor(u => u.ModifiedAt,   f  => f.Date.Recent().ToUniversalTime());

        var mods = modFaker.Generate(3);
        context.Users.AddRange(mods);
        await context.SaveChangesAsync();
        return mods;
    }

    private static async Task<List<User>> GenerateFreelancersAsync(AppDbContext context, Faker faker,
        string defaultPasswordHash, Role freelancerRole, List<Country> countries, List<Skill> skills, List<Language> languages, Guid adminId)
    {
        var freelancerFaker = new Faker<User>("en")
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Email,        f  => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _  => defaultPasswordHash)
            .RuleFor(u => u.DisplayName,  f  => f.Name.FullName())
            .RuleFor(u => u.RoleId,       _  => freelancerRole.Id)
            .RuleFor(u => u.CountryId,    f  => f.PickRandom(countries).Id)
            .RuleFor(u => u.AvatarImg,    f  => f.Internet.Avatar())
            .RuleFor(u => u.CreatedBy,    _  => adminId)
            .RuleFor(u => u.ModifiedBy,   _  => adminId)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past(2).ToUniversalTime())
            .RuleFor(u => u.ModifiedAt,   f  => f.Date.Recent().ToUniversalTime());

        var testFreelancer = freelancerFaker.Clone()
            .RuleFor(u => u.Email,       _ => "freelancer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Freelancer")
            .Generate();

        var freelancersUsers = freelancerFaker.Generate(50);
        freelancersUsers.Add(testFreelancer);
        context.Users.AddRange(freelancersUsers);

        // Згенеруємо мови для фрілансерів
        foreach(var user in freelancersUsers)
        {
            var userLangs = faker.PickRandom(languages, faker.Random.Int(1, 3)).Distinct().ToList();
            foreach(var l in userLangs)
            {
                context.UserLanguages.Add(new UserLanguage { UserId = user.Id, LanguageId = l.Id, ProficiencyLevel = faker.PickRandom<ProficiencyLevel>() });
            }
        }
        await context.SaveChangesAsync();

        context.Freelancers.AddRange(freelancersUsers.Select(u => new Freelancer
        {
            Id         = u.Id,
            Bio        = faker.Lorem.Paragraphs(2),
            Location   = faker.Address.City(),
            Skills     = faker.PickRandom(skills, faker.Random.Int(3, 8)).Distinct().ToList(),
            CreatedBy  = u.Id,
            CreatedAt  = u.CreatedAt,
            ModifiedBy = u.Id,
            ModifiedAt = u.ModifiedAt
        }));
        
        // Згенеруємо портфоліо
        foreach(var u in freelancersUsers)
        {
            var portfoliosCount = faker.Random.Int(1, 6);
            for(int i=0; i<portfoliosCount; i++)
            {
                context.Portfolios.Add(new Portfolio
                {
                    Id = Guid.NewGuid(),
                    FreelancerId = u.Id,
                    Title = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Paragraph(),
                    PortfolioUrl = faker.Internet.Url(),
                    CreatedBy = u.Id,
                    CreatedAt = u.CreatedAt.AddDays(faker.Random.Int(1, 30)),
                    ModifiedBy = u.Id,
                    ModifiedAt = u.ModifiedAt
                });
            }
        }

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
        string defaultPasswordHash, Role employerRole, List<Country> countries, List<Language> languages, Guid adminId)
    {
        var employerFaker = new Faker<User>("en")
            .RuleFor(u => u.Id,           _  => Guid.NewGuid())
            .RuleFor(u => u.Email,        f  => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _  => defaultPasswordHash)
            .RuleFor(u => u.DisplayName,  f  => f.Name.FullName())
            .RuleFor(u => u.RoleId,       _  => employerRole.Id)
            .RuleFor(u => u.CountryId,    f  => f.PickRandom(countries).Id)
            .RuleFor(u => u.AvatarImg,    f  => f.Internet.Avatar())
            .RuleFor(u => u.CreatedBy,    _  => adminId)
            .RuleFor(u => u.ModifiedBy,   _  => adminId)
            .RuleFor(u => u.CreatedAt,    f  => f.Date.Past(2).ToUniversalTime())
            .RuleFor(u => u.ModifiedAt,   f  => f.Date.Recent().ToUniversalTime());

        var testEmployer = employerFaker.Clone()
            .RuleFor(u => u.Email,       _ => "employer@test.com")
            .RuleFor(u => u.DisplayName, _ => "Test Employer")
            .Generate();

        var employerUsers = employerFaker.Generate(20);
        employerUsers.Add(testEmployer);
        context.Users.AddRange(employerUsers);
        
        foreach(var user in employerUsers)
        {
            var userLangs = faker.PickRandom(languages, faker.Random.Int(1, 2)).Distinct().ToList();
            foreach(var l in userLangs)
            {
                context.UserLanguages.Add(new UserLanguage { UserId = user.Id, LanguageId = l.Id, ProficiencyLevel = faker.PickRandom<ProficiencyLevel>() });
            }
        }
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
            .RuleFor(p => p.Description, f  => f.Lorem.Paragraphs(f.Random.Int(1, 4)))
            .RuleFor(p => p.Budget,      f  => Math.Round(f.Random.Decimal(100, 10000), 2))
            .RuleFor(p => p.Status,      f  => f.PickRandom(projectStatuses))
            .RuleFor(p => p.Deadline,    f  => f.Date.Future().ToUniversalTime())
            .RuleFor(p => p.CreatedBy,   f  => f.PickRandom(employerUsers).Id)
            .RuleFor(p => p.CreatedAt,   f  => f.Date.Past().ToUniversalTime())
            .RuleFor(p => p.ModifiedBy, (_, p) => p.CreatedBy)
            .RuleFor(p => p.ModifiedAt,  f  => f.Date.Recent().ToUniversalTime());

        var projects = projectFaker.Generate(150);

        foreach (var project in projects)
            project.Categories = faker.PickRandom(categories, faker.Random.Int(1, 4)).Distinct().ToList();

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        return projects;
    }

    private static async Task GenerateProjectMilestonesAsync(AppDbContext context, Faker faker,
        List<Project> projects)
    {
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
            var count = faker.Random.Int(2, 5);
            var shares = Enumerable.Range(0, count).Select(_ => faker.Random.Double(0.1)).ToList();
            var total = shares.Sum();
            var amounts = shares
                .Select(s => Math.Round((decimal)(s / total) * project.Budget * faker.Random.Decimal(0.8m), 2))
                .ToList();

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

        context.Bids.AddRange(bidFaker.Generate(300));
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

        context.Quotes.AddRange(quoteFaker.Generate(100));
        await context.SaveChangesAsync();
    }

    private static async Task<List<Contract>> GenerateContractsAsync(AppDbContext context, Faker faker, List<Project> projects,
        List<User> freelancersUsers, List<User> employerUsers)
    {
        var inProgressProjects = projects
            .Where(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Completed).ToList();

        if (!inProgressProjects.Any()) return new List<Contract>();

        var contractFaker = new Faker<Contract>("en")
            .RuleFor(c => c.Id,           _  => Guid.NewGuid())
            .RuleFor(c => c.ProjectId,    f  => f.PickRandom(inProgressProjects).Id)
            .RuleFor(c => c.FreelancerId, f  => f.PickRandom(freelancersUsers).Id)
            .RuleFor(c => c.StartDate,    f  => f.Date.Past().ToUniversalTime())
            .RuleFor(c => c.AgreedRate,   (f, c) =>
                Math.Round(f.Random.Decimal(100, inProgressProjects.First(x => x.Id == c.ProjectId).Budget), 2))
            .RuleFor(c => c.Status,       (_, c) => inProgressProjects.First(x => x.Id == c.ProjectId).Status == ProjectStatus.Completed ? ContractStatus.Completed : ContractStatus.Active)
            .RuleFor(c => c.CreatedBy,    f  => f.PickRandom(employerUsers).Id)
            .RuleFor(c => c.CreatedAt,    (_, c) => c.StartDate)
            .RuleFor(c => c.ModifiedBy,   (_, c) => c.CreatedBy)
            .RuleFor(c => c.ModifiedAt,   (_, c) => c.StartDate);

        var contracts = contractFaker.Generate(inProgressProjects.Count); // One contract per project
        
        // Match contract CreatedBy with Project CreatedBy
        foreach(var c in contracts)
        {
            var p = inProgressProjects.First(x => x.Id == c.ProjectId);
            c.CreatedBy = p.CreatedBy;
            c.ModifiedBy = p.CreatedBy;
        }
        
        context.Contracts.AddRange(contracts);
        
        // Generate Contract Milestones
        var allProjectMilestones = await context.ProjectMilestones.ToListAsync();
        var contractMilestones = new List<ContractMilestone>();
        
        foreach(var c in contracts)
        {
            var pMilestones = allProjectMilestones.Where(m => m.ProjectId == c.ProjectId).ToList();
            foreach(var pm in pMilestones)
            {
                ContractMilestoneStatus status;
                if (c.Status == ContractStatus.Completed)
                {
                    status = ContractMilestoneStatus.Approved; // Or Approved
                }
                else
                {
                    status = faker.PickRandom<ContractMilestoneStatus>();
                }
                
                contractMilestones.Add(new ContractMilestone {
                    Id = Guid.NewGuid(),
                    ContractId = c.Id,
                    Description = pm.Description,
                    Amount = pm.Amount,
                    DueDate = pm.DueDate,
                    Status = status,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = c.CreatedAt,
                    ModifiedBy = c.CreatedBy,
                    ModifiedAt = c.CreatedAt
                });
            }
        }
        context.ContractMilestones.AddRange(contractMilestones);
        await context.SaveChangesAsync();
        
        return contracts;
    }

    private static async Task GenerateMessagesAsync(AppDbContext context, Faker faker, List<Contract> contracts)
    {
        var messages = new List<Message>();
        foreach(var contract in contracts)
        {
            var msgCount = faker.Random.Int(5, 20);
            var startDate = contract.StartDate;
            
            for(int i=0; i<msgCount; i++)
            {
                bool isFreelancerSender = faker.Random.Bool();
                Guid senderId = isFreelancerSender ? contract.FreelancerId : contract.CreatedBy;
                Guid receiverId = isFreelancerSender ? contract.CreatedBy : contract.FreelancerId;
                
                messages.Add(new Message {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ReceiverId = receiverId,
                    Text = faker.Lorem.Sentence(faker.Random.Int(3, 15)),
                    SentAt = startDate.AddDays(i).AddHours(faker.Random.Int(1, 10)),
                    CreatedBy = senderId,
                    CreatedAt = startDate.AddDays(i).AddHours(faker.Random.Int(1, 10)),
                    ModifiedBy = senderId,
                    ModifiedAt = startDate.AddDays(i).AddHours(faker.Random.Int(1, 10))
                });
            }
        }
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
    }

    private static async Task GenerateReviewsAsync(AppDbContext context, Faker faker, List<Contract> contracts, int freelancerRoleId, int employerRoleId)
    {
        var completedContracts = contracts.Where(c => c.Status == ContractStatus.Completed).ToList();
        var reviews = new List<Review>();
        
        foreach(var contract in completedContracts)
        {
            // Employer reviews freelancer
            reviews.Add(new Review {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                ReviewedUserId = contract.FreelancerId,
                Rating = faker.Random.Decimal(3.5m, 5.0m),
                ReviewText = faker.Lorem.Paragraph(),
                ReviewerRoleId = employerRoleId,
                CreatedBy = contract.CreatedBy,
                CreatedAt = contract.StartDate.AddMonths(1),
                ModifiedBy = contract.CreatedBy,
                ModifiedAt = contract.StartDate.AddMonths(1)
            });
            
            // Freelancer reviews employer
            reviews.Add(new Review {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                ReviewedUserId = contract.CreatedBy,
                Rating = faker.Random.Decimal(4.0m, 5.0m),
                ReviewText = faker.Lorem.Paragraph(),
                ReviewerRoleId = freelancerRoleId,
                CreatedBy = contract.FreelancerId,
                CreatedAt = contract.StartDate.AddMonths(1).AddDays(1),
                ModifiedBy = contract.FreelancerId,
                ModifiedAt = contract.StartDate.AddMonths(1).AddDays(1)
            });
        }
        
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }

    private static async Task GeneratePaymentsAndTransactionsAsync(AppDbContext context, Faker faker, List<Contract> contracts)
    {
        var completedContracts = contracts.Where(c => c.Status == ContractStatus.Completed).ToList();
        var contractMilestones = await context.ContractMilestones.ToListAsync();
        
        var wallets = await context.UserWallets.ToListAsync();
        
        foreach(var contract in completedContracts)
        {
            var milestones = contractMilestones.Where(m => m.ContractId == contract.Id).ToList();
            
            var employerWallet = wallets.FirstOrDefault(w => w.CreatedBy == contract.CreatedBy);
            var freelancerWallet = wallets.FirstOrDefault(w => w.CreatedBy == contract.FreelancerId);
            
            foreach(var m in milestones)
            {
                // Create Payment
                context.ContractPayments.Add(new ContractPayment {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    MilestoneId = m.Id,
                    Amount = m.Amount,
                    PaymentDate = m.DueDate.AddDays(faker.Random.Int(1, 3)),
                    PaymentMethod = "Credit Card"
                });
                
                // Wallet Transactions (Withdraw from Employer, Deposit to Freelancer)
                if (employerWallet != null)
                {
                    context.WalletTransactions.Add(new WalletTransaction {
                        Id = Guid.NewGuid(),
                        WalletId = employerWallet.Id,
                        Amount = -m.Amount,
                        TransactionType = "EscrowRelease",
                        TransactionDate = m.DueDate.AddDays(1)
                    });
                }
                
                if (freelancerWallet != null)
                {
                    context.WalletTransactions.Add(new WalletTransaction {
                        Id = Guid.NewGuid(),
                        WalletId = freelancerWallet.Id,
                        Amount = m.Amount * 0.9m, // Platform fee
                        TransactionType = "Income",
                        TransactionDate = m.DueDate.AddDays(2)
                    });
                }
            }
        }
        
        await context.SaveChangesAsync();
    }

    private static async Task GenerateDisputesAsync(AppDbContext context, Faker faker, List<Contract> contracts, List<User> moderators)
    {
        var disputeContracts = faker.PickRandom(contracts, 10).ToList();
        var disputes = new List<Dispute>();
        var resolutions = new List<DisputeResolution>();
        
        foreach(var c in disputeContracts)
        {
            var dispute = new Dispute {
                Id = Guid.NewGuid(),
                ContractId = c.Id,
                Reason = faker.Lorem.Sentence(10),
                Status = faker.PickRandom<DisputeStatus>(),
                CreatedBy = faker.Random.Bool() ? c.FreelancerId : c.CreatedBy,
                CreatedAt = c.StartDate.AddDays(faker.Random.Int(10, 30)),
                ModifiedBy = c.CreatedBy,
                ModifiedAt = c.StartDate.AddDays(faker.Random.Int(10, 30))
            };
            disputes.Add(dispute);
            
            if (dispute.Status == DisputeStatus.Resolved || dispute.Status == DisputeStatus.Rejected)
            {
                var mod = faker.PickRandom(moderators);
                resolutions.Add(new DisputeResolution {
                    Id = Guid.NewGuid(),
                    DisputeId = dispute.Id,
                    ResolutionDetails = faker.Lorem.Paragraph(),
                    CreatedBy = mod.Id,
                    CreatedAt = dispute.CreatedAt.AddDays(faker.Random.Int(2, 7)),
                    ModifiedBy = mod.Id,
                    ModifiedAt = dispute.CreatedAt.AddDays(faker.Random.Int(2, 7))
                });
            }
        }
        
        context.Disputes.AddRange(disputes);
        context.DisputeResolutions.AddRange(resolutions);
        await context.SaveChangesAsync();
    }

    private static async Task GenerateNotificationsAsync(AppDbContext context, Faker faker, List<User> freelancers, List<User> employers)
    {
        var users = freelancers.Concat(employers).ToList();
        var notifications = new List<Notification>();
        
        foreach(var user in users)
        {
            var notifCount = faker.Random.Int(5, 15);
            for(int i=0; i<notifCount; i++)
            {
                notifications.Add(new Notification {
                    Id = Guid.NewGuid(),
                    Message = faker.Lorem.Sentence(),
                    Type = faker.PickRandom<NotificationType>(),
                    IsRead = faker.Random.Bool(),
                    SentAt = faker.Date.Recent(30).ToUniversalTime(),
                    UserId = user.Id,
                    LinkAddress = faker.Internet.Url()
                });
            }
        }
        
        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }
}