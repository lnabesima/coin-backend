namespace Coin.API.UnitTests.Controllers;

using Coin.API.Controllers;
using Coin.API.Extensions;
using Coin.Application.DTOs.Transactions;
using Coin.Application.Interfaces;
using Coin.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class TransactionsControllerTests
{
    private readonly ITransactionService _transactionService = Substitute.For<ITransactionService>();
    private readonly Guid _userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        _controller = new TransactionsController(_transactionService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        _controller.HttpContext.Items[HttpContextExtensions.UserIdItemKey] = _userId;
    }

    private static TransactionResponseDto CreateSampleResponse(Guid? id = null, Guid? userId = null)
    {
        return new TransactionResponseDto(
            id ?? Guid.NewGuid(),
            userId ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Supermarket",
            150.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food,
            -150.0m,
            DateTime.UtcNow);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtActionWithLocationAndBody()
    {
        // Arrange
        CreateTransactionDto dto = new(
            "Supermarket",
            150.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        TransactionResponseDto expectedResponse = CreateSampleResponse(userId: _userId);
        _transactionService.CreateAsync(_userId, dto, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(TransactionsController.GetById), createdResult.ActionName);
        Assert.Equal(expectedResponse.Id, createdResult.RouteValues?["id"]);
        Assert.Equal(expectedResponse, createdResult.Value);
    }

    [Fact]
    public async Task Create_WhenDomainThrowsArgumentException_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        CreateTransactionDto dto = new(
            "Invalid",
            -50.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        _transactionService.CreateAsync(_userId, dto, Arg.Any<CancellationToken>())
            .Throws(new ArgumentOutOfRangeException("amount", "Amount must be strictly greater than zero."));

        // Act
        IActionResult result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Contains("Amount must be strictly greater than zero", problem.Detail);
    }

    [Fact]
    public async Task GetAll_WithNoDateFilters_ReturnsOkWithList()
    {
        // Arrange
        List<TransactionResponseDto> list = [CreateSampleResponse(), CreateSampleResponse()];
        _transactionService.GetAllAsync(_userId, null, null, Arg.Any<CancellationToken>())
            .Returns(list);

        // Act
        IActionResult result = await _controller.GetAll(null, null, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public async Task GetAll_WithDateFilters_PassesFiltersToService()
    {
        // Arrange
        DateTime start = DateTime.UtcNow.AddDays(-7);
        DateTime end = DateTime.UtcNow;
        List<TransactionResponseDto> list = [CreateSampleResponse()];

        _transactionService.GetAllAsync(_userId, start, end, Arg.Any<CancellationToken>())
            .Returns(list);

        // Act
        IActionResult result = await _controller.GetAll(start, end, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        await _transactionService.Received(1).GetAllAsync(_userId, start, end, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_WhenTransactionExists_ReturnsOkWithItem()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        TransactionResponseDto response = CreateSampleResponse(id: id, userId: _userId);
        _transactionService.GetByIdAsync(_userId, id, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        IActionResult result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _transactionService.GetByIdAsync(_userId, id, Arg.Any<CancellationToken>())
            .Returns((TransactionResponseDto?)null);

        // Act
        IActionResult result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Contains(id.ToString(), problem.Detail);
    }

    [Fact]
    public async Task Update_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateTransactionDto dto = new(
            "Updated Description",
            200.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Housing);

        TransactionResponseDto updatedResponse = CreateSampleResponse(id: id, userId: _userId);
        _transactionService.UpdateAsync(_userId, id, dto, Arg.Any<CancellationToken>())
            .Returns(updatedResponse);

        // Act
        IActionResult result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateTransactionDto dto = new(
            "Updated Description",
            200.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Housing);

        _transactionService.UpdateAsync(_userId, id, dto, Arg.Any<CancellationToken>())
            .Returns((TransactionResponseDto?)null);

        // Act
        IActionResult result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task Update_WhenDomainThrowsArgumentException_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdateTransactionDto dto = new(
            "",
            200.0m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Housing);

        _transactionService.UpdateAsync(_userId, id, dto, Arg.Any<CancellationToken>())
            .Throws(new ArgumentException("Description cannot be empty.", "description"));

        // Act
        IActionResult result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Contains("Description cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _transactionService.DeleteAsync(_userId, id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        IActionResult result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _transactionService.DeleteAsync(_userId, id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        IActionResult result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Found", problem.Title);
        Assert.Contains(id.ToString(), problem.Detail);
    }
}
