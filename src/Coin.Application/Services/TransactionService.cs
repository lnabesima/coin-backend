namespace Coin.Application.Services;

using Coin.Application.DTOs.Transactions;
using Coin.Application.Interfaces;
using Coin.Domain.Entities;

public class TransactionService(ITransactionRepository repository) : ITransactionService
{
    private readonly ITransactionRepository _repository = repository;

    public async Task<TransactionResponseDto> CreateAsync(
        Guid userId,
        CreateTransactionDto dto,
        CancellationToken cancellationToken = default)
    {
        Transaction transaction = new(
            userId,
            dto.Description,
            dto.Amount,
            dto.Date,
            dto.Type,
            dto.Category);

        await _repository.AddAsync(transaction, cancellationToken);

        return TransactionResponseDto.FromDomain(transaction);
    }

    public async Task<TransactionResponseDto?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await _repository.GetByIdAsync(userId, id, cancellationToken);

        return transaction is null ? null : TransactionResponseDto.FromDomain(transaction);
    }

    public async Task<IReadOnlyList<TransactionResponseDto>> GetAllAsync(
        Guid userId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Transaction> transactions = await _repository.GetAllAsync(userId, startDate, endDate, cancellationToken);

        return [.. transactions.Select(TransactionResponseDto.FromDomain)];
    }

    public async Task<TransactionResponseDto?> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateTransactionDto dto,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await _repository.GetByIdAsync(userId, id, cancellationToken);

        if (transaction is null)
            return null;

        transaction.Update(
            dto.Description,
            dto.Amount,
            dto.Date,
            dto.Type,
            dto.Category);

        await _repository.UpdateAsync(transaction, cancellationToken);

        return TransactionResponseDto.FromDomain(transaction);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await _repository.GetByIdAsync(userId, id, cancellationToken);

        if (transaction is null)
            return false;

        await _repository.DeleteAsync(transaction, cancellationToken);

        return true;
    }
}
