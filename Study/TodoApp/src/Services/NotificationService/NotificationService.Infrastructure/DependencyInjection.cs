using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Consumers;
using NotificationService.Application.Sagas;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<Data.NotificationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(Data.NotificationDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

        services.AddMassTransit(cfg =>
        {
            // Saga state machine — stored in PostgreSQL for durability
            cfg.AddSagaStateMachine<TodoAssignmentStateMachine, TodoAssignmentState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, Data.NotificationDbContext>((sp, builder) =>
                        builder.UseNpgsql(
                            configuration.GetConnectionString("DefaultConnection"),
                            npgsql => npgsql.MigrationsAssembly(typeof(Data.NotificationDbContext).Assembly.FullName)));
                });

            // Inbox for idempotent consumers
            cfg.AddEntityFrameworkOutbox<Data.NotificationDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            // Consumers
            cfg.AddConsumer<TodoCreatedConsumer>();
            cfg.AddConsumer<TodoUpdatedConsumer>();
            cfg.AddConsumer<TodoCompletedConsumer>();
            cfg.AddConsumer<TodoDeletedConsumer>();
            cfg.AddConsumer<TodoAssignedConsumer>();
            cfg.AddConsumer<UserRegisteredProjectionConsumer>();
            cfg.AddConsumer<UserProfileUpdatedProjectionConsumer>();

            cfg.UsingRabbitMq((ctx, rmq) =>
            {
                rmq.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });

                rmq.UseMessageRetry(r =>
                    r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(3)));

                rmq.UseKillSwitch(options =>
                {
                    options.SetActivationThreshold(10);
                    options.SetTripThreshold(0.15);
                    options.SetRestartTimeout(m: 1);
                });

                rmq.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<Data.NotificationDbContext>().Database.MigrateAsync();
    }
}
