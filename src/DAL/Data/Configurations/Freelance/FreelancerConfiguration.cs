using DAL.Extensions;
using Domain.Models.Freelance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Freelance;

public class FreelancerConfiguration : IEntityTypeConfiguration<Freelancer>
{
    public void Configure(EntityTypeBuilder<Freelancer> builder)
    {
        builder.ToTable("freelancers");
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Bio).HasMaxLength(2000);
        builder.Property(p => p.Location).HasMaxLength(128);
        
        builder.HasMany(p => p.Skills)
            .WithMany()
            .UsingEntity(join => join.ToTable("freelancers_skills"));

        builder.ConfigureAudit(DeleteBehavior.Cascade);
    }
}