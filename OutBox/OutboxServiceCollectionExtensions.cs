using Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OutBox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionalOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<TDbContext>());
        services.AddScoped<IMessageBroker, OutBox>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
