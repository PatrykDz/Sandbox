# CLAUDE.md — TodoApp Microservices Architecture Guide

This document is written for AI assistants to understand the codebase conventions, architecture patterns, and development workflows. Read this before modifying any code.

---

## Project Purpose

**TodoApp** is an architecture reference implementation — an "ultra beautiful" example of production-grade .NET 9 microservices using:

- **Clean Architecture** with strict layer dependency enforcement
- **Domain-Driven Design (DDD)** — aggregates, value objects, strongly-typed IDs, domain events
- **CQRS** via MediatR with pipeline behaviors
- **Outbox/Inbox pattern** via MassTransit + PostgreSQL for guaranteed message delivery
- **Full observability** via OpenTelemetry → OTel Collector → Grafana (Tempo + Loki + Prometheus)

This is a guide project. Quality, correctness, and architectural clarity are paramount. Prefer explicitness over cleverness.

---

## Solution Map

```
TodoApp/
├── src/
│   ├── Shared/
│   │   ├── Shared.Contracts/          # Integration event records (shared between services)
│   │   └── Shared.BuildingBlocks/     # Base classes, CQRS interfaces, Result<T>, PagedResult<T>
│   └── Services/
│       ├── TodoService/               # Todo aggregate management
│       ├── UserService/               # User registration and profile
│       ├── AuditService/              # Append-only audit trail (event-sourced read model)
│       └── NotificationService/       # Notification delivery (worker + saga)
│
├── infrastructure/
│   ├── postgres/init.sql              # DB extensions, pgcrypto, performance settings
│   ├── rabbitmq/                      # rabbitmq.conf + enabled_plugins
│   ├── otel-collector/otelcol.yml     # OTel Collector: OTLP in → Tempo/Prometheus/Loki out
│   ├── prometheus/prometheus.yml      # Scrape config for services + postgres + rabbitmq
│   ├── loki/loki.yml                  # Log aggregation backend config
│   ├── tempo/tempo.yml                # Distributed tracing backend config
│   └── grafana/
│       ├── provisioning/              # Auto-provision datasources + dashboard providers
│       └── dashboards/                # Pre-built dashboard JSON files
│
├── k8s/                               # Kubernetes manifests (HPA, NetworkPolicy, etc.)
├── docker-compose.yml                 # Full stack: infra + services + observability
├── docker-compose.override.yml        # Development mode overrides
└── .env.example                       # Environment variable template
```

Each service follows this four-project structure:

```
ServiceName/
├── ServiceName.Domain/          # net9.0 class library — ZERO external dependencies
├── ServiceName.Application/     # net9.0 class library — MediatR, FluentValidation, MassTransit abstractions
├── ServiceName.Infrastructure/  # net9.0 class library — EF Core, MassTransit, OpenTelemetry
└── ServiceName.API/             # Microsoft.NET.Sdk.Web — composition root
```

NotificationService replaces `.API` with `.Worker` (SDK: `Microsoft.NET.Sdk.Worker`).

---

## Architecture Rules (Enforce These Always)

### Dependency Direction

```
API/Worker  →  Application  →  Domain
Infrastructure  →  Application (implements interfaces)
```

- **Domain** has NO NuGet dependencies. Only references `Shared.BuildingBlocks`.
- **Application** references Domain + Shared.Contracts + Shared.BuildingBlocks. Knows about MediatR, FluentValidation, MassTransit abstractions.
- **Infrastructure** references Application + Domain. Implements repository interfaces, configures EF Core and MassTransit.
- **API/Worker** references Application + Infrastructure. Is the composition root — wires everything together in `Program.cs`.

**Never break this rule.** The domain must compile with zero infrastructure/framework dependencies.

### Layer Content Rules

| What | Where |
|------|-------|
| Business invariants, aggregate state changes | Domain |
| Command/query handlers, validators, behaviors | Application |
| Domain event handlers that publish integration events | Application `DomainEventHandlers/` |
| EF Core configurations, DbContext, migrations | Infrastructure `Data/` |
| Repository implementations | Infrastructure `Repositories/` |
| MassTransit configuration, outbox, consumers | Infrastructure `DependencyInjection.cs` |
| OpenTelemetry setup | Infrastructure `ObservabilityExtensions.cs` |
| Controllers, middleware, health checks, `Program.cs` | API |

---

## Key Building Blocks

### `Shared.BuildingBlocks`

| Type | Usage |
|------|-------|
| `AggregateRoot<TId>` | Base for aggregate roots; manages `_domainEvents` list |
| `Entity<TId>` | Base for all entities; provides `Id` property |
| `ValueObject` | Base for value objects; structural equality via `GetEqualityComponents()` |
| `IDomainEvent` | Marker interface — all domain events implement this |
| `ICommand` / `ICommandHandler<,>` | MediatR-based command CQRS interfaces |
| `IQuery<T>` / `IQueryHandler<,>` | MediatR-based query CQRS interfaces |
| `Result<T>` / `Result` | Railway-oriented result type — replaces exceptions for domain failures |
| `Error` | Typed error with `Code`, `Description`, and `ErrorType` |
| `PagedResult<T>` | Pagination wrapper: `Items`, `TotalCount`, `Page`, `PageSize`, `TotalPages` |
| `Specification<T>` | Composable query specification with `.ToExpression()` |
| `IUnitOfWork` | Abstraction for `SaveChangesAsync` |

### `Shared.Contracts`

Contains all **integration events** — records shared between services via RabbitMQ:

| Event | Published by | Consumed by |
|-------|-------------|-------------|
| `TodoCreatedEvent` | TodoService | AuditService, NotificationService |
| `TodoUpdatedEvent` | TodoService | AuditService, NotificationService |
| `TodoCompletedEvent` | TodoService | AuditService, NotificationService |
| `TodoDeletedEvent` | TodoService | AuditService |
| `TodoAssignedEvent` | TodoService | AuditService, NotificationService |
| `UserRegisteredEvent` | UserService | AuditService, NotificationService |
| `UserProfileUpdatedEvent` | UserService | AuditService |
| `NotificationSentEvent` | NotificationService | _(none currently)_ |

---

## Service Details

### TodoService

- **Aggregate**: `Todo` with strongly-typed `TodoId(Guid)` and `UserId(Guid)`
- **Value objects**: `TodoTitle` (max 200 chars), `TodoDescription` (max 2000 chars)
- **Enums**: `TodoStatus` (Pending/InProgress/Completed/Cancelled), `TodoPriority` (Low/Medium/High/Critical)
- **History**: `TodoHistoryEntry` child entities track every state change atomically (saved in same EF Core transaction)
- **Domain events**: `TodoCreatedDomainEvent`, `TodoUpdatedDomainEvent`, `TodoCompletedDomainEvent`, `TodoCancelledDomainEvent`, `TodoDeletedDomainEvent`, `TodoAssignedDomainEvent`, `TodoStatusChangedDomainEvent`
- **Commands**: CreateTodo, UpdateTodo, DeleteTodo, CompleteTodo, StartProgress, CancelTodo, AssignTodo, AddTag, RemoveTag
- **Queries**: GetTodo, GetTodos (paginated, filterable), GetTodoHistory (paginated per-todo audit trail)
- **API endpoints**: `GET/POST /api/v1/todos`, `GET/PUT/DELETE /api/v1/todos/{id}`, `POST /api/v1/todos/{id}/complete|start-progress|cancel|assign`, `GET/POST/DELETE /api/v1/todos/{id}/tags`, `GET /api/v1/todos/{id}/history`
- **Port**: 5000 (Docker), 8080 (internal)

### UserService

- **Aggregate**: `User` with `Guid` ID
- **Value objects**: `Email`, `FullName` (FirstName, LastName)
- **Enum**: `UserStatus` (Active/Deactivated)
- **Domain events**: `UserRegisteredDomainEvent`, `UserProfileUpdatedDomainEvent`, `UserDeactivatedDomainEvent`
- **Commands**: RegisterUser, UpdateProfile, DeactivateUser
- **Queries**: GetUser, GetUsers
- **Port**: 5001

### AuditService

- **Entity**: `AuditRecord` — immutable append-only record (EventType, AggregateType, AggregateId, Payload JSON, OccurredAt, ServiceSource)
- **Consumers**: TodoCreatedAuditConsumer, TodoUpdatedAuditConsumer, TodoCompletedAuditConsumer, TodoDeletedAuditConsumer, UserRegisteredAuditConsumer
- **Queries**: GetAuditTrail (filterable by entityId, eventType, date range)
- **Port**: 5002

### NotificationService

- **Pattern**: Background Worker (no HTTP API)
- **Consumers**: TodoCreatedConsumer, TodoUpdatedConsumer, TodoCompletedConsumer, TodoAssignedConsumer, UserProjectionConsumers
- **Saga**: `TodoAssignmentStateMachine` — orchestrates assignment notification flow
- **No port** (worker only)

---

## Domain Event Flow

```
1. Aggregate.SomeOperation() mutates state + calls RaiseDomainEvent(new MyDomainEvent(...))
2. CommandHandler calls await unitOfWork.SaveChangesAsync()
3. EF Core SaveChanges executes → aggregate written to DB
4. DomainEventDispatcherInterceptor.SavedChangesAsync fires (AFTER save)
5. For each domain event → mediator.Publish(domainEvent)
6. Handler: MyDomainEventHandler calls publishEndpoint.Publish(new IntegrationEvent(...))
   └─ This writes to MassTransit outbox table (same DB, second SaveChanges)
7. MassTransit outbox dispatcher polls DB and forwards messages to RabbitMQ
8. Consuming services receive messages through their consumers
```

> **Important**: Domain events are dispatched AFTER `SaveChanges`. This is intentional — it ensures the aggregate state is committed before integration events are published. Side-effect handlers (e.g., history recording) are handled differently — `TodoHistoryEntry` child entities are added to the aggregate's `_historyEntries` list and saved atomically with the aggregate in step 3.

---

## How to Add a New Feature

### Adding a new Command

1. Create `src/Services/ServiceName/ServiceName.Domain/` — add method to aggregate if needed
2. Create `ServiceName.Application/Commands/MyCommand/`:
   - `MyCommand.cs` — `public sealed record MyCommand(...) : IRequest<Result<SomeDto>>;`
   - `MyCommandHandler.cs` — `internal sealed class MyCommandHandler(...) : IRequestHandler<MyCommand, Result<SomeDto>>`
   - `MyCommandValidator.cs` — optional FluentValidation validator
3. Add endpoint to the Controller

**Handler pattern**:
```csharp
internal sealed class MyCommandHandler(ITodoRepository repo, IUnitOfWork uow, ILogger<MyCommandHandler> logger)
    : IRequestHandler<MyCommand, Result>
{
    public async Task<Result> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var todo = await repo.GetByIdAsync(TodoId.From(request.TodoId), cancellationToken);
        if (todo is null) return Error.NotFound("Todo.NotFound", $"Todo {request.TodoId} not found.");

        var result = todo.SomeOperation(request.SomeParam);
        if (result.IsFailure) return result.Error;

        repo.Update(todo);
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

### Adding a new Query

1. Create `ServiceName.Application/Queries/GetSomething/`:
   - `GetSomethingQuery.cs` — `public sealed record GetSomethingQuery(...) : IRequest<Result<SomeDto>>;`
   - `GetSomethingQueryHandler.cs` — use `AsNoTracking()`, never mutate state in queries

2. Add endpoint to the Controller

### Adding a new Domain Event + Integration Event

1. **Domain event**: Add to `ServiceName.Domain/Events/ServiceDomainEvents.cs`
   ```csharp
   public sealed record MyDomainEvent(Guid EventId, DateTime OccurredAt, ...) : IDomainEvent;
   ```

2. **Raise in aggregate**: `RaiseDomainEvent(new MyDomainEvent(Guid.NewGuid(), DateTime.UtcNow, ...));`

3. **Integration event**: Add to `Shared.Contracts/Events/MyEvent.cs`
   ```csharp
   public record MyEvent(Guid EventId, DateTime OccurredAt, string EventVersion, ...) {
       public MyEvent(...) : this(Guid.NewGuid(), DateTime.UtcNow, "1.0", ...) { }
   }
   ```

4. **Domain event handler**: Create `ServiceName.Application/DomainEventHandlers/MyDomainEventHandler.cs`
   ```csharp
   internal sealed class MyDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<...> logger)
       : INotificationHandler<MyDomainEvent>
   {
       public async Task Handle(MyDomainEvent notification, CancellationToken ct)
           => await publishEndpoint.Publish(new MyEvent(...), ct);
   }
   ```

5. **Add consumer** in the target service's `Application/Consumers/` folder

### Adding a new Service

Follow the four-project pattern. Key checklist:
- Domain: aggregate, value objects, domain events, `IMyRepository`, `IUnitOfWork`
- Application: commands, queries, validators, behaviors, `DependencyInjection.cs` with `AddApplication()`
- Infrastructure: `DbContext`, EF configurations, repository implementations, `ObservabilityExtensions.cs`, `DependencyInjection.cs` with `AddInfrastructure(IConfiguration)` and `ApplyMigrationsAsync()`
- API: `Program.cs`, `appsettings.json`, `Dockerfile`, controllers
- Add all projects to `TodoApp.sln` (use the existing solution folder GUIDs as a pattern)
- Add service to `docker-compose.yml`

---

## EF Core Conventions

- **Table names**: `snake_case` (e.g., `todo_history_entries`)
- **Column names**: `snake_case` (e.g., `assigned_to_user_id`)
- **All configurations**: `IEntityTypeConfiguration<T>` in `Data/Configurations/` — never use data annotations
- **Strongly-typed IDs**: always use `HasConversion(id => id.Value, value => TypedId.From(value))`
- **Value objects**: use `OwnsOne` with `builder.OwnsOne(e => e.ValueObjectProp, vo => { ... })`
- **Migrations**: auto-applied on service startup via `ApplyMigrationsAsync()` — never run migrations manually in Docker
- **Retry**: all DbContexts configured with `npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)`
- **Indexes**: always define indexes for foreign keys and common query predicates

### DbContext Structure

Every service's `DbContext` includes MassTransit tables for the Outbox/Inbox pattern:
```csharp
modelBuilder.AddInboxStateEntity();
modelBuilder.AddOutboxMessageEntity();
modelBuilder.AddOutboxStateEntity();
```

---

## MassTransit / Messaging Conventions

- **Outbox**: Every service that publishes events uses `cfg.AddEntityFrameworkOutbox<TDbContext>(...)` — guarantees at-least-once delivery
- **Retry policy**: Exponential back-off on all consumers: `Exponential(5, 1s, 60s, 3s)`
- **Kill switch** (consumers only): Trips at 15% error rate after 10 messages; restarts after 1 minute
- **Consumer naming**: `{EventType}Consumer` (e.g., `TodoCreatedAuditConsumer`)
- **Exchange topology**: MassTransit auto-generates exchanges and queues from consumer and message type names

---

## OpenTelemetry Setup

Each service exports **traces, metrics, and logs** to the OTel Collector at `http://otel-collector:4317` (OTLP gRPC).

**Configuration location**: Infrastructure layer `ObservabilityExtensions.cs` — called from `AddInfrastructure()`.

**What is instrumented**:
- ASP.NET Core HTTP requests (with exception recording, health check filtering)
- HTTP client calls
- Entity Framework Core queries (with SQL statement capture)
- Npgsql database commands
- MassTransit publish/consume (`AddSource("MassTransit")`, `AddMeter("MassTransit")`)
- .NET runtime metrics (GC, threadpool)
- Process metrics

**OTLP config in appsettings**:
```json
{ "Otlp": { "Endpoint": "http://otel-collector:4317" } }
```

**Observability stack ports**:
| Service | Port | URL |
|---------|------|-----|
| Grafana | 3000 | `http://localhost:3000` (admin/admin) |
| Prometheus | 9090 | `http://localhost:9090` |
| Loki | 3100 | internal only |
| Tempo | 3200 | internal only |
| OTel Collector | 4317/4318 | OTLP receiver |

---

## Result Pattern

**Always use `Result<T>` / `Result` for business failures** — never throw exceptions for expected domain errors.

```csharp
// Domain method returns Result
public Result Complete()
{
    if (Status == TodoStatus.Completed) return TodoErrors.AlreadyCompleted;
    // ...
    return Result.Success();
}

// Handler checks result
var result = todo.Complete();
if (result.IsFailure) return result.Error;

// Controller maps to HTTP
return result.ToActionResult(this);
// → 200/201 on success, 400/404/422/500 based on ErrorType
```

**Error types**:
- `Error.NotFound(code, description)` → 404
- `Error.Validation(code, description)` → 400
- `Error.Conflict(code, description)` → 422
- `Error.Unauthorized(code, description)` → 401
- `Error.Failure(code, description)` → 500

Predefined errors live in `ServiceName.Domain/Errors/ServiceErrors.cs`.

---

## Naming Conventions

| Concept | Convention | Example |
|---------|-----------|---------|
| Aggregate root | `ClassName` (sealed) | `Todo`, `User` |
| Strongly-typed ID | `ConceptId` record struct | `TodoId`, `UserId` |
| Value object | `ConceptName` | `Email`, `TodoTitle` |
| Domain event | `ConceptVerbDomainEvent` | `TodoCreatedDomainEvent` |
| Integration event | `ConceptVerbEvent` | `TodoCreatedEvent` |
| Command | `VerbConceptCommand` | `CreateTodoCommand` |
| Query | `GetConceptQuery` | `GetTodoQuery` |
| Handler | same as message + `Handler` | `CreateTodoCommandHandler` |
| Validator | same as command + `Validator` | `CreateTodoCommandValidator` |
| Repository interface | `IConceptRepository` | `ITodoRepository` |
| Repository impl | `ConceptRepository` | `TodoRepository` |
| EF config | `ConceptConfiguration` | `TodoConfiguration` |
| Consumer | `EventTypeConsumer` | `TodoCreatedAuditConsumer` |

All handlers and repositories are `internal sealed class` — never `public`.

---

## Running Locally

### Prerequisites
- Docker + Docker Compose v2
- .NET 9 SDK

### Full stack (recommended)
```bash
cp .env.example .env
docker compose up --build
```

**Service URLs after startup:**
- TodoService API: http://localhost:5000
- TodoService Scalar docs: http://localhost:5000/scalar (development only)
- UserService API: http://localhost:5001
- AuditService API: http://localhost:5002
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- Grafana: http://localhost:3000 (admin/admin)
- Prometheus: http://localhost:9090

### Infra only (for local .NET development)
```bash
docker compose up postgres rabbitmq otel-collector prometheus loki tempo grafana -d
cd src/Services/TodoService/TodoService.API && dotnet run
```

### Run a single service
```bash
# From repo root
dotnet run --project src/Services/TodoService/TodoService.API
```

---

## Common Operations

### Adding EF Core Migration

```bash
# Run from repo root
dotnet ef migrations add MigrationName \
  --project src/Services/TodoService/TodoService.Infrastructure \
  --startup-project src/Services/TodoService/TodoService.API \
  --output-dir Data/Migrations
```

Migrations are auto-applied on startup — no manual `database update` needed.

### Checking health
```bash
curl http://localhost:5000/health       # liveness
curl http://localhost:5000/health/ready # readiness
```

### Viewing logs in Grafana
1. Open http://localhost:3000
2. Go to Explore → select Loki datasource
3. Query: `{job="todo-service"} |= "error"`

### Viewing traces in Grafana
1. Open http://localhost:3000
2. Go to Explore → select Tempo datasource
3. Search by service name or trace ID

---

## Important Files Reference

| File | Purpose |
|------|---------|
| `TodoApp.sln` | Visual Studio solution — all 13 projects listed |
| `docker-compose.yml` | Full stack definition including observability |
| `infrastructure/otel-collector/otelcol.yml` | OTel Collector pipeline: OTLP → Tempo/Prometheus/Loki |
| `infrastructure/grafana/provisioning/datasources/all.yml` | Auto-provisioned Prometheus, Loki, Tempo datasources |
| `infrastructure/grafana/dashboards/services-overview.json` | Pre-built services dashboard |
| `src/Shared/Shared.Contracts/Events/` | All integration event contracts |
| `src/Shared/Shared.BuildingBlocks/` | Base classes used by every service |
| `src/Services/TodoService/TodoService.Domain/Entities/Todo.cs` | Reference implementation of a DDD aggregate |
| `src/Services/TodoService/TodoService.Infrastructure/DependencyInjection.cs` | Reference for EF Core + MassTransit + OTEL wiring |

---

## Testing Guidance

This project does not currently have unit/integration tests (it is a reference architecture). When adding tests:

- **Unit tests**: Test domain entities in isolation — no infrastructure needed. Test `Result<T>` values.
- **Integration tests**: Use `WebApplicationFactory<Program>` with `Testcontainers` for PostgreSQL and RabbitMQ.
- **Consumer tests**: Use `MassTransit.Testing` harness for in-memory consumer testing.

Test project naming convention: `ServiceName.Domain.Tests`, `ServiceName.Application.Tests`, `ServiceName.Integration.Tests`.

---

## Things to Avoid

- **Never** add `using` directives for Infrastructure namespaces in Domain or Application projects
- **Never** inject `DbContext` directly into Application layer handlers — always use repository interfaces
- **Never** throw exceptions for expected business rule failures — use `Result<T>`
- **Never** use `async void` — always `async Task`
- **Never** use `SaveChanges()` (sync) — always `SaveChangesAsync()`
- **Never** skip `AsNoTracking()` in query handlers
- **Never** commit real secrets — `.env` is in `.gitignore`, use `.env.example` as template
- **Never** use `_` (discard) for awaited operations that may fail silently
- **Avoid** calling `SaveChangesAsync` multiple times in a single handler — use `IUnitOfWork` at the end
