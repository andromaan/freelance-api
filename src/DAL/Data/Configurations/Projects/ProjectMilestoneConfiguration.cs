using DAL.Extensions;
using Domain.Models.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Projects;

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.ToTable("project_milestones");
        
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Description).HasMaxLength(2000);
        builder.Property(pm => pm.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(pm => pm.DueDate).IsRequired();

        builder.HasOne(pm => pm.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.ConfigureAudit();
    }
}