using DAL.Extensions;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Messaging;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Text).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.SentAt).IsRequired();
        builder.Property(p => p.IsRead).HasDefaultValue(false);
        builder.Property(p => p.IsEdited).HasDefaultValue(false);

        builder.HasOne(p => p.Contract)
            .WithMany()
            .HasForeignKey(p => p.ContractId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Receiver)
            .WithMany()
            .HasForeignKey(p => p.ReceiverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ConfigureAudit();
    }
}