namespace Coin.Application.Interfaces;

using Coin.Application.DTOs.Transactions;

public interface ITransactionService
{
    Task<TransactionResponseDto> CreateAsync(
        Guid userId,
        CreateTransactionDto dto,
        CancellationToken cancellationToken = default);

    Task<TransactionResponseDto?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionResponseDto>> GetAllAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<TransactionResponseDto?> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateTransactionDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);
}
