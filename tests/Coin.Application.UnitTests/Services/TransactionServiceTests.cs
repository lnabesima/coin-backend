namespace Coin.Application.UnitTests.Services;

using Coin.Application.DTOs.Transactions;
using Coin.Application.Interfaces;
using Coin.Application.Services;
using Coin.Domain.Entities;
using Coin.Domain.Enums;
using NSubstitute;
using Xunit;

public class TransactionServiceTests
{
    private readonly ITransactionRepository _repository;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _repository = Substitute.For<ITransactionRepository>();
        _service = new TransactionService(_repository);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_PersistsAndReturnsResponseDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateTransactionDto(
            "Grocery Shopping",
            120.50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        var result = await _service.CreateAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.Amount, result.Amount);
        Assert.Equal(dto.Type, result.Type);
        Assert.Equal(dto.Category, result.Category);
        Assert.Equal(-120.50m, result.SignedAmount);

        await _repository.Received(1).AddAsync(
            Arg.Is<Transaction>(t => t.UserId == userId && t.Description == dto.Description && t.Amount == dto.Amount),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateAsync_WithInvalidAmount_ThrowsArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateTransactionDto(
            "Invalid Amount",
            invalidAmount,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.CreateAsync(userId, dto));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithEmptyDescription_ThrowsArgumentException(string invalidDescription)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateTransactionDto(
            invalidDescription,
            50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(userId, dto));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionExistsAndBelongsToUser_ReturnsResponseDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction(
            userId,
            "Salary",
            5000m,
            DateTime.UtcNow,
            TransactionType.Income,
            TransactionCategory.Salary,
            transactionId);

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns(transaction);

        // Act
        var result = await _service.GetByIdAsync(userId, transactionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Salary", result.Description);
        Assert.Equal(5000m, result.SignedAmount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionNotFoundOrBelongsToAnotherUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);

        // Act
        var result = await _service.GetByIdAsync(userId, transactionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenTransactionsExist_ReturnsMappedList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var transactions = new List<Transaction>
        {
            new(userId, "Coffee", 5.00m, DateTime.UtcNow.AddDays(-2), TransactionType.Expense, TransactionCategory.Food),
            new(userId, "Freelance", 300.00m, DateTime.UtcNow.AddDays(-1), TransactionType.Income, TransactionCategory.Other)
        };

        _repository.GetAllAsync(userId, startDate, endDate, Arg.Any<CancellationToken>())
            .Returns(transactions);

        // Act
        var results = await _service.GetAllAsync(userId, startDate, endDate);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal("Coffee", results[0].Description);
        Assert.Equal(-5.00m, results[0].SignedAmount);
        Assert.Equal("Freelance", results[1].Description);
        Assert.Equal(300.00m, results[1].SignedAmount);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTransactionsExist_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repository.GetAllAsync(userId, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<Transaction>());

        // Act
        var results = await _service.GetAllAsync(userId);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionExistsAndBelongsToUser_UpdatesAndReturnsResponseDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction(
            userId,
            "Old Description",
            100m,
            DateTime.UtcNow.AddDays(-1),
            TransactionType.Expense,
            TransactionCategory.Other,
            transactionId);

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns(transaction);

        var updateDto = new UpdateTransactionDto(
            "New Description",
            150m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        var result = await _service.UpdateAsync(userId, transactionId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(150m, result.Amount);
        Assert.Equal(TransactionCategory.Food, result.Category);

        await _repository.Received(1).UpdateAsync(transaction, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);

        var updateDto = new UpdateTransactionDto(
            "New Description",
            150m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        var result = await _service.UpdateAsync(userId, transactionId, updateDto);

        // Assert
        Assert.Null(result);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenTransactionExistsAndBelongsToUser_DeletesAndReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction(
            userId,
            "To Delete",
            20m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Other,
            transactionId);

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns(transaction);

        // Act
        var result = await _service.DeleteAsync(userId, transactionId);

        // Assert
        Assert.True(result);
        await _repository.Received(1).DeleteAsync(transaction, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenTransactionNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        _repository.GetByIdAsync(userId, transactionId, Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);

        // Act
        var result = await _service.DeleteAsync(userId, transactionId);

        // Assert
        Assert.False(result);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }
}
