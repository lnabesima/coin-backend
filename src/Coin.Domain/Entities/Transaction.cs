namespace Coin.Domain.Entities;

using Coin.Domain.Enums;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionCategory Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public decimal SignedAmount => Type == TransactionType.Income ? Amount : -Amount;

    private Transaction() { }

    public Transaction(
        Guid userId,
        string description,
        decimal amount,
        DateTime date,
        TransactionType type,
        TransactionCategory category,
        Guid? id = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty or whitespace.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be strictly greater than zero.");

        if (!Enum.IsDefined(type))
            throw new ArgumentException($"Invalid transaction type: {type}.", nameof(type));

        if (!Enum.IsDefined(category))
            throw new ArgumentException($"Invalid transaction category: {category}.", nameof(category));

        Id = id ?? Guid.NewGuid();
        UserId = userId;
        Description = description.Trim();
        Amount = amount;
        Date = date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
        Type = type;
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string description,
        decimal amount,
        DateTime date,
        TransactionType type,
        TransactionCategory category)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty or whitespace.", nameof(description));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be strictly greater than zero.");

        if (!Enum.IsDefined(type))
            throw new ArgumentException($"Invalid transaction type: {type}.", nameof(type));

        if (!Enum.IsDefined(category))
            throw new ArgumentException($"Invalid transaction category: {category}.", nameof(category));

        Description = description.Trim();
        Amount = amount;
        Date = date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
        Type = type;
        Category = category;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
