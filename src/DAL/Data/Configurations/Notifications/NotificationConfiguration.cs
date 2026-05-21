using DAL.Converters;
using Domain.Models.Notifications;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(cm => cm.Type).HasMaxLength(32)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationType)Enum.Parse(typeof(NotificationType), v)).IsRequired();
        
        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.LinkAddress)
            .IsRequired(false);
        
        builder.Property(n => n.SentAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())");
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}