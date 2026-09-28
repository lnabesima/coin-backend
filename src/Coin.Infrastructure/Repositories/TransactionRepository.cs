namespace Coin.Infrastructure.Repositories;

using Coin.Application.Interfaces;
using Coin.Domain.Entities;
using Coin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class TransactionRepository(CoinDbContext context) : ITransactionRepository
{
    private readonly CoinDbContext _context = context;

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            var startUtc = startDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : startDate.Value.ToUniversalTime();

            query = query.Where(t => t.Date >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = endDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : endDate.Value.ToUniversalTime();

            query = query.Where(t => t.Date <= endUtc);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.Delete();
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
