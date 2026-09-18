using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


await app.StartAsync();
string url = builder.Configuration["urls"]?
    .Split(';')
    .FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    ?? "https://localhost:7000";
Console.WriteLine($"Aplicação iniciada em {url}.");
await app.WaitForShutdownAsync();

public partial class Program { }
