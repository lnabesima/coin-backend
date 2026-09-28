namespace Coin.API.Extensions;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
        });

        return services;
    }
}

public sealed class ApiKeySecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "API Key authentication header"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["ApiKey"] = scheme;

        var schemeRef = new OpenApiSecuritySchemeReference("ApiKey", document);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [schemeRef] = []
        });

        return Task.CompletedTask;
    }
}
