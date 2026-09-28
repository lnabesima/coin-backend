namespace Coin.API.Extensions;

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsExtensions
{
    public static ObjectResult NotFoundProblem(this ControllerBase controller, string detail) =>
        controller.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: detail,
            type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4");

    public static ObjectResult BadRequestProblem(this ControllerBase controller, string detail) =>
        controller.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Bad Request",
            detail: detail,
            type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1");

    public static async Task WriteUnauthorizedProblemAsync(this HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = detail,
            Instance = context.Request.Path
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails, cancellationToken: context.RequestAborted);
    }
}
