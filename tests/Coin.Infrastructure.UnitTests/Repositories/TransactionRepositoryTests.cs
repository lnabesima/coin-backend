namespace Coin.Infrastructure.UnitTests.Repositories;

using Coin.Domain.Entities;
using Coin.Domain.Enums;
using Coin.Infrastructure.Persistence;
using Coin.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class TransactionRepositoryTests : IDisposable
{
    private readonly CoinDbContext _context;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests()
    {
        DbContextOptions<CoinDbContext> options = new DbContextOptionsBuilder<CoinDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CoinDbContext(options);
        _repository = new TransactionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_PersistsTransactionToDatabase()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Transaction transaction = new(
            userId,
            "Groceries",
            120.50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        await _repository.AddAsync(transaction);

        // Assert
        Transaction? persisted = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(persisted);
        Assert.Equal(userId, persisted.UserId);
        Assert.Equal("Groceries", persisted.Description);
        Assert.Equal(120.50m, persisted.Amount);
        Assert.Equal(TransactionType.Expense, persisted.Type);
        Assert.Equal(TransactionCategory.Food, persisted.Category);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMatchesUserAndId_ReturnsTransaction()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Transaction transaction = new(
            userId,
            "Salary",
            3000m,
            DateTime.UtcNow,
            TransactionType.Income,
            TransactionCategory.Salary);

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        Transaction? result = await _repository.GetByIdAsync(userId, transaction.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Salary", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionBelongsToAnotherUser_ReturnsNull()
    {
        // Arrange
        Guid ownerUserId = Guid.NewGuid();
        Guid requestingUserId = Guid.NewGuid();
        Transaction transaction = new(
            ownerUserId,
            "Confidential Expense",
            500m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Other);

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        Transaction? result = await _repository.GetByIdAsync(requestingUserId, transaction.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNonExistent_ReturnsNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        Transaction? result = await _repository.GetByIdAsync(userId, Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyTransactionsForSpecifiedUser_OrderedByDateDescending()
    {
        // Arrange
        Guid user1 = Guid.NewGuid();
        Guid user2 = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Transaction t1 = new(user1, "Oldest", 10m, now.AddDays(-5), TransactionType.Expense, TransactionCategory.Food);
        Transaction t2 = new(user1, "Newest", 20m, now.AddDays(-1), TransactionType.Expense, TransactionCategory.Food);
        Transaction t3 = new(user1, "Middle", 15m, now.AddDays(-3), TransactionType.Expense, TransactionCategory.Food);
        Transaction otherUserTransaction = new(user2, "Other", 99m, now.AddDays(-2), TransactionType.Expense, TransactionCategory.Other);

        await _context.Transactions.AddRangeAsync(t1, t2, t3, otherUserTransaction);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<Transaction> results = await _repository.GetAllAsync(user1);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, t => Assert.Equal(user1, t.UserId));
        Assert.Equal("Newest", results[0].Description);
        Assert.Equal("Middle", results[1].Description);
        Assert.Equal("Oldest", results[2].Description);
    }

    [Fact]
    public async Task GetAllAsync_WithDateFilters_AppliesRangeCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Transaction t1 = new(userId, "Day 1", 10m, now.AddDays(-10), TransactionType.Expense, TransactionCategory.Food);
        Transaction t2 = new(userId, "Day 5", 20m, now.AddDays(-5), TransactionType.Expense, TransactionCategory.Food);
        Transaction t3 = new(userId, "Day 3", 30m, now.AddDays(-3), TransactionType.Expense, TransactionCategory.Food);
        Transaction t4 = new(userId, "Today", 40m, now, TransactionType.Expense, TransactionCategory.Food);

        await _context.Transactions.AddRangeAsync(t1, t2, t3, t4);
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<Transaction> results = await _repository.GetAllAsync(userId, startDate: now.AddDays(-6), endDate: now.AddDays(-2));

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Day 3", results[0].Description);
        Assert.Equal("Day 5", results[1].Description);
    }

    [Fact]
    public async Task GetAllAsync_WhenUserHasNoTransactions_ReturnsEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        IReadOnlyList<Transaction> results = await _repository.GetAllAsync(userId);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTransactionInDatabase()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Transaction transaction = new(
            userId,
            "Initial Description",
            50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        transaction.Update("Updated Description", 75m, DateTime.UtcNow, TransactionType.Expense, TransactionCategory.Housing);
        await _repository.UpdateAsync(transaction);

        // Assert
        Transaction? updated = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Description", updated.Description);
        Assert.Equal(75m, updated.Amount);
        Assert.Equal(TransactionCategory.Housing, updated.Category);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesTransactionInDatabase()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Transaction transaction = new(
            userId,
            "To Delete",
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Other);

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        var beforeDelete = DateTime.UtcNow;

        // Act
        await _repository.DeleteAsync(transaction);

        // Assert - query with global filter returns null
        Transaction? deleted = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.Null(deleted);

        // Assert - query ignoring global filter shows record exists with IsDeleted = true
        Transaction? softDeleted = await _context.Transactions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);

        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
        Assert.NotNull(softDeleted.DeletedAt);
        Assert.True(softDeleted.DeletedAt >= beforeDelete);
    }
}
