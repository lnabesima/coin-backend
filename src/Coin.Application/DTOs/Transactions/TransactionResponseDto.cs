 namespace Coin.Application.DTOs.Transactions;

using Coin.Domain.Entities;
using Coin.Domain.Enums;

public record TransactionResponseDto(
    Guid Id,
    Guid UserId,
    string Description,
    decimal Amount,
    DateTime Date,
    TransactionType Type,
    TransactionCategory Category,
    decimal SignedAmount,
    DateTime CreatedAt)
{
    public static TransactionResponseDto FromDomain(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.UserId,
            transaction.Description,
            transaction.Amount,
            transaction.Date,
            transaction.Type,
            transaction.Category,
            transaction.SignedAmount,
            transaction.CreatedAt);
}
