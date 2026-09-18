namespace Coin.Domain.UnitTests;

using Coin.Domain.Entities;
using Coin.Domain.Enums;
using Xunit;

public class TransactionTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTransactionSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var description = "Grocery shopping";
        var amount = 150.75m;
        var date = DateTime.UtcNow;
        var type = TransactionType.Expense;
        var category = TransactionCategory.Food;

        // Act
        var transaction = new Transaction(userId, description, amount, date, type, category);

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(userId, transaction.UserId);
        Assert.Equal(description, transaction.Description);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(type, transaction.Type);
        Assert.Equal(category, transaction.Category);
        Assert.True(transaction.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithCustomId_ShouldPreserveIdAndGenerateUtcNowCreatedAt()
    {
        // Arrange
        var customId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(
            userId,
            "Salary",
            5000m,
            DateTime.UtcNow,
            TransactionType.Income,
            TransactionCategory.Salary,
            id: customId);

        // Assert
        Assert.Equal(customId, transaction.Id);
        Assert.True(transaction.CreatedAt >= beforeCreation);
        Assert.True(transaction.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.Empty,
            "Rent",
            1200m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Housing));

        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhitespaceDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.NewGuid(),
            invalidDescription!,
            50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Leisure));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimDescription()
    {
        // Arrange
        var untrimmedDescription = "  Bus ticket  ";

        // Act
        var transaction = new Transaction(
            Guid.NewGuid(),
            untrimmedDescription,
            4.50m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Transportation);

        // Assert
        Assert.Equal("Bus ticket", transaction.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Constructor_WithZeroOrNegativeAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Transaction(
            Guid.NewGuid(),
            "Coffee",
            invalidAmount,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidType_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.NewGuid(),
            "Invalid Type",
            100m,
            DateTime.UtcNow,
            (TransactionType)999,
            TransactionCategory.Other));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidCategory_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Transaction(
            Guid.NewGuid(),
            "Invalid Category",
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            (TransactionCategory)999));

        Assert.Equal("category", exception.ParamName);
    }

    [Fact]
    public void SignedAmount_WhenIncome_ShouldReturnPositiveAmount()
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(),
            "Dividends",
            250m,
            DateTime.UtcNow,
            TransactionType.Income,
            TransactionCategory.Other);

        // Act & Assert
        Assert.Equal(250m, transaction.SignedAmount);
    }

    [Fact]
    public void SignedAmount_WhenExpense_ShouldReturnNegativeAmount()
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(),
            "Pharmacy",
            80m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Health);

        // Act & Assert
        Assert.Equal(-80m, transaction.SignedAmount);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(),
            "Initial Description",
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        var updatedDate = DateTime.UtcNow.AddDays(1);

        // Act
        transaction.Update(
            "Updated Description",
            200m,
            updatedDate,
            TransactionType.Income,
            TransactionCategory.Salary);

        // Assert
        Assert.Equal("Updated Description", transaction.Description);
        Assert.Equal(200m, transaction.Amount);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(TransactionCategory.Salary, transaction.Category);
        Assert.Equal(200m, transaction.SignedAmount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithNullOrWhitespaceDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(),
            "Valid Description",
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => transaction.Update(
            invalidDescription!,
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food));

        Assert.Equal("description", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Update_WithZeroOrNegativeAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(),
            "Valid Description",
            100m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => transaction.Update(
            "Valid Description",
            invalidAmount,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food));

        Assert.Equal("amount", exception.ParamName);
    }
}
