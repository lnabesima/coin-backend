using Coin.API.Authentication;
using Coin.API.Configuration;
using Coin.API.Extensions;
using Coin.Application;
using Coin.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddOptions<ApiKeyAuthenticationOptions>()
    .Bind(builder.Configuration.GetSection(ApiKeyAuthenticationOptions.SectionName))
    .Validate(options =>
    {
        if (builder.Environment.IsDevelopment())
            return !string.IsNullOrWhiteSpace(options.ApiKey);

        return !string.IsNullOrWhiteSpace(options.ApiKey) && options.DefaultUserId != Guid.Empty;
    }, "Authentication:ApiKey and Authentication:DefaultUserId must be configured.")
    .ValidateOnStart();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme,
        options =>
        {
            builder.Configuration.GetSection(ApiKeyAuthenticationOptions.SectionName).Bind(options);
        });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiDocumentation();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(CorsOptions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();


await app.StartAsync();
string url = builder.Configuration["urls"]?
    .Split(';')
    .FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    ?? "https://localhost:7000";
Console.WriteLine($"Aplicação iniciada em {url}.");
await app.WaitForShutdownAsync();

public partial class Program { }
