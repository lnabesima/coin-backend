namespace Coin.API.Controllers;

using Coin.API.Extensions;
using Coin.Application.DTOs.Transactions;
using Coin.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();

        try
        {
            TransactionResponseDto result = await _transactionService.CreateAsync(userId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return this.BadRequestProblem(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();
        IReadOnlyList<TransactionResponseDto> result = await _transactionService.GetAllAsync(
            userId,
            startDate,
            endDate,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetById))]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();
        TransactionResponseDto? result = await _transactionService.GetByIdAsync(userId, id, cancellationToken);

        if (result is null)
        {
            return this.NotFoundProblem($"Transaction with ID '{id}' was not found.");
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();

        try
        {
            TransactionResponseDto? result = await _transactionService.UpdateAsync(
                userId,
                id,
                dto,
                cancellationToken);

            if (result is null)
            {
                return this.NotFoundProblem($"Transaction with ID '{id}' was not found.");
            }

            return NoContent();
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            return this.BadRequestProblem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();
        bool deleted = await _transactionService.DeleteAsync(userId, id, cancellationToken);

        if (!deleted)
        {
            return this.NotFoundProblem($"Transaction with ID '{id}' was not found.");
        }

        return NoContent();
    }
}
