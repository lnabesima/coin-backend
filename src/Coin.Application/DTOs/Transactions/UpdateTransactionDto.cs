namespace Coin.Application.DTOs.Transactions;

using Coin.Domain.Enums;

public record UpdateTransactionDto(
    string Description,
    decimal Amount,
    DateTime Date,
    TransactionType Type,
    TransactionCategory Category);
