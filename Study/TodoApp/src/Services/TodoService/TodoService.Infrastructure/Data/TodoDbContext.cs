using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TodoService.Domain.Entities;
using TodoService.Infrastructure.Data.Interceptors;

namespace TodoService.Infrastructure.Data;

public sealed class TodoDbContext(
    DbContextOptions<TodoDbContext> options,
    DomainEventDispatcherInterceptor domainEventInterceptor)
    : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<TodoTag> TodoTags => Set<TodoTag>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(domainEventInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // MassTransit Outbox / Inbox tables — stored alongside domain data
        // for transactional consistency via the outbox pattern
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
