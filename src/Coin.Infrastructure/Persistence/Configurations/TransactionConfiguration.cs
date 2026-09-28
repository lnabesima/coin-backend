namespace Coin.Infrastructure.Persistence.Configurations;

using Coin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.Category)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt);

        builder.Ignore(t => t.SignedAmount);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasIndex(t => new { t.UserId, t.Date })
            .HasDatabaseName("IX_Transactions_UserId_Date");
    }
}
