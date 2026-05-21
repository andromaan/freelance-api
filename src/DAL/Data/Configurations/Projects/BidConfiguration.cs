using Domain.Models.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Projects;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("bids");
        
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.Message).IsRequired(false);
        builder.Property(b => b.IsInteresting).HasDefaultValue(null);

        builder.HasOne(b => b.Project)
            .WithMany(b => b.Bids)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(b => b.Freelancer)
            .WithMany()
            .HasForeignKey(b => b.FreelancerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}