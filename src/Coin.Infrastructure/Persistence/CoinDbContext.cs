namespace Coin.Infrastructure.Persistence;

using Coin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CoinDbContext : DbContext
{
    public CoinDbContext(DbContextOptions<CoinDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoinDbContext).Assembly);
    }
}
