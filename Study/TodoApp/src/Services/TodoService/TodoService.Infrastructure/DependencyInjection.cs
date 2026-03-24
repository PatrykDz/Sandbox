using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoService.Domain.Repositories;
using TodoService.Infrastructure.Data;
using TodoService.Infrastructure.Data.Interceptors;
using TodoService.Infrastructure.Repositories;

namespace TodoService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Domain event dispatcher interceptor — scoped so it can resolve IMediator
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddDbContext<TodoDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(TodoDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    npgsql.CommandTimeout(30);
                });
        });

        services.AddScoped<IUnitOfWork, TodoUnitOfWork>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoHistoryRepository, TodoHistoryRepository>();

        services.AddMassTransit(cfg =>
        {
            // Outbox: messages are stored in the same PostgreSQL DB
            // as domain data. Delivered to RabbitMQ by the outbox dispatcher.
            cfg.AddEntityFrameworkOutbox<TodoDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox(busOutbox =>
                {
                    busOutbox.MessageDeliveryLimit = 100;
                });
                outbox.QueryDelay = TimeSpan.FromMilliseconds(500);
            });

            cfg.UsingRabbitMq((ctx, rmq) =>
            {
                rmq.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });

                rmq.UseMessageRetry(r =>
                    r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(3)));

                rmq.ConfigureEndpoints(ctx);
            });
        });

        services.AddObservability(configuration);

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
