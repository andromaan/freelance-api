using System.Reflection;
using DAL.Data.Initializer;
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

namespace DAL.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Freelancer> Freelancers { get; set; }
    public DbSet<Employer> Employers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<DisputeResolution> DisputeResolutions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ContractPayment> ContractPayments { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractMilestone> ContractMilestones { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserWallet> UserWallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<UserLanguage> UserLanguages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
        DataSeed.SeedData(builder);
    }
}