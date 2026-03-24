using AuditService.Application.Consumers;
using AuditService.Domain.Repositories;
using AuditService.Infrastructure.Data;
using AuditService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => { npgsql.MigrationsAssembly(typeof(AuditDbContext).Assembly.FullName); npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); }));

        services.AddScoped<IAuditRepository, AuditRepository>();

        services.AddMassTransit(cfg =>
        {
            // Inbox: ensures each message is processed exactly once (idempotent consumers)
            cfg.AddEntityFrameworkOutbox<AuditDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            cfg.AddConsumer<TodoCreatedAuditConsumer>();
            cfg.AddConsumer<TodoUpdatedAuditConsumer>();
            cfg.AddConsumer<TodoCompletedAuditConsumer>();
            cfg.AddConsumer<TodoDeletedAuditConsumer>();
            cfg.AddConsumer<UserRegisteredAuditConsumer>();

            cfg.UsingRabbitMq((ctx, rmq) =>
            {
                rmq.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });
                rmq.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(3)));
                rmq.UseKillSwitch(options => { options.SetActivationThreshold(10); options.SetTripThreshold(0.15); options.SetRestartTimeout(m: 1); });
                rmq.ConfigureEndpoints(ctx);
            });
        });

        services.AddObservability(configuration);

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AuditDbContext>().Database.MigrateAsync();
    }
}
