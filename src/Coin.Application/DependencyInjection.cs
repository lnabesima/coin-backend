namespace Coin.Application;

using Coin.Application.Interfaces;
using Coin.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
