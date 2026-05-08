using DAL.Converters;
using Domain.Models.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations.Payments;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");
        
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(wt => wt.TransactionType).HasMaxLength(64).IsRequired();
        builder.Property(wt => wt.TransactionDate)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())");
            

        builder.HasOne(wt => wt.Wallet)
            .WithMany()
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}