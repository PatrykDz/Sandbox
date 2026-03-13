using MassTransit;
using NotificationService.Application.Consumers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog((cfg) =>
    cfg.ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithThreadId()
       .WriteTo.Console(outputTemplate:
           "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<TodoCreatedConsumer>();
    cfg.AddConsumer<TodoUpdatedConsumer>();
    cfg.AddConsumer<TodoCompletedConsumer>();
    cfg.AddConsumer<TodoDeletedConsumer>();

    cfg.UsingRabbitMq((ctx, rmq) =>
    {
        rmq.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]!);
            h.Password(builder.Configuration["RabbitMq:Password"]!);
        });

        rmq.UseMessageRetry(r =>
        {
            r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2));
        });

        rmq.UseKillSwitch(options =>
        {
            options.SetActivationThreshold(10);
            options.SetTripThreshold(0.15);
            options.SetRestartTimeout(m: 1);
        });

        rmq.ConfigureEndpoints(ctx);
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString:
        $"amqp://{builder.Configuration["RabbitMq:Username"]}:{builder.Configuration["RabbitMq:Password"]}@{builder.Configuration["RabbitMq:Host"]}");

var host = builder.Build();
await host.RunAsync();
