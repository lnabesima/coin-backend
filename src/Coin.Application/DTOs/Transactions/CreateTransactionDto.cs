namespace Coin.Application.DTOs.Transactions;

using Coin.Domain.Enums;

public record CreateTransactionDto(
    string Description,
    decimal Amount,
    DateTime Date,
    TransactionType Type,
    TransactionCategory Category);
