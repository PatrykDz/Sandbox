# TodoApp — Distributed Microservices

A production-grade, highly scalable distributed TODO API built with **.NET 9**, **PostgreSQL**, **RabbitMQ**, **MassTransit Outbox**, **Clean Architecture**, and **CQRS**.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  Client  ──►  Ingress (nginx)  ──►  TodoService API                          │
│                                        │                                      │
│                                        ▼                                      │
│                                   PostgreSQL  ◄──► MassTransit Outbox        │
│                                        │                    │                 │
│                                        │                    ▼                 │
│                                        │              RabbitMQ Bus            │
│                                        │                    │                 │
│                                        │                    ▼                 │
│                                        │         NotificationService Worker   │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Outbox Pattern (Guaranteed Delivery)
Messages are written to the **same PostgreSQL transaction** as domain changes.
The MassTransit outbox dispatcher then forwards them to RabbitMQ asynchronously,
ensuring **no message is lost** even if the broker is temporarily unavailable.

---

## Solution Structure

```
TodoApp/
├── src/
│   ├── Shared/
│   │   ├── Shared.Contracts/          # Message contracts (events) shared between services
│   │   └── Shared.BuildingBlocks/     # Base classes: Entity, AggregateRoot, CQRS interfaces
│   │
│   └── Services/
│       ├── TodoService/
│       │   ├── TodoService.Domain/        # Entities, Domain Events, Repository interfaces
│       │   ├── TodoService.Application/   # CQRS Commands/Queries, Validators, Behaviors
│       │   ├── TodoService.Infrastructure/# EF Core, MassTransit, Repository implementations
│       │   └── TodoService.API/           # ASP.NET Core controllers, middleware, Program.cs
│       │
│       └── NotificationService/
│           ├── NotificationService.Application/  # MassTransit consumers
│           └── NotificationService.Worker/       # Worker host, DI wiring
│
├── infrastructure/
│   ├── postgres/init.sql              # DB extensions & performance settings
│   └── rabbitmq/rabbitmq.conf         # Broker configuration
│
├── k8s/
│   ├── namespace.yaml
│   ├── secrets.yaml                   # Template (never commit real secrets)
│   ├── network-policy.yaml            # Least-privilege pod communication
│   ├── hpa.yaml                       # Horizontal Pod Autoscalers
│   ├── ingress.yaml                   # nginx ingress
│   ├── infrastructure/
│   │   ├── postgres-deployment.yaml   # StatefulSet
│   │   ├── postgres-service.yaml
│   │   ├── rabbitmq-deployment.yaml   # StatefulSet
│   │   └── rabbitmq-service.yaml
│   └── services/
│       ├── todo-service-configmap.yaml
│       ├── todo-service-deployment.yaml
│       ├── todo-service-service.yaml
│       ├── notification-service-configmap.yaml
│       └── notification-service-deployment.yaml
│
├── docker-compose.yml                 # Production-like local stack
├── docker-compose.override.yml        # Development overrides
└── .env.example                       # Environment variable template
```

---

## Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Business logic, aggregates, domain events, repository contracts |
| **Application** | CQRS handlers, FluentValidation, pipeline behaviors (logging, validation) |
| **Infrastructure** | EF Core + PostgreSQL, MassTransit + RabbitMQ, outbox, migrations |
| **API / Worker** | HTTP/worker host, DI composition root, error handling |

---

## Quick Start

### Prerequisites
- Docker Desktop / Docker Engine + Compose v2
- .NET 9 SDK (for local development)

### Run with Docker Compose

```bash
# Copy env file
cp .env.example .env

# Start full stack (builds images, starts postgres + rabbitmq + services)
docker compose up --build

# API available at:
# http://localhost:5000/api/v1/todos
# http://localhost:5000/scalar   (API docs)
# http://localhost:15672         (RabbitMQ Management - guest/guest)
```

### Local Development (without Docker for services)

```bash
# Start only infrastructure
docker compose up postgres rabbitmq -d

# Run TodoService
cd src/Services/TodoService/TodoService.API
dotnet run

# Run NotificationService (in another terminal)
cd src/Services/NotificationService/NotificationService.Worker
dotnet run
```

---

## API Reference

### Todos

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/v1/todos` | List todos (paginated, filterable) |
| `GET` | `/api/v1/todos/{id}` | Get todo by ID |
| `POST` | `/api/v1/todos` | Create todo |
| `PUT` | `/api/v1/todos/{id}` | Update todo |
| `POST` | `/api/v1/todos/{id}/complete` | Mark as completed |
| `DELETE` | `/api/v1/todos/{id}` | Delete todo |

### Query Parameters (GET /api/v1/todos)

| Param | Type | Description |
|-------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20) |
| `status` | enum | Pending, InProgress, Completed, Cancelled |
| `priority` | enum | Low, Medium, High, Critical |
| `assignedToUserId` | guid | Filter by assignee |
| `searchTerm` | string | Full-text search on title/description |

---

## Kubernetes Deployment

```bash
# Create namespace and secrets first
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.yaml          # Edit with real secrets first!

# Infrastructure
kubectl apply -f k8s/infrastructure/

# Application services
kubectl apply -f k8s/services/

# Ingress, HPA, Network policies
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml
kubectl apply -f k8s/network-policy.yaml

# Check status
kubectl get all -n todoapp
```

---

## Design Decisions

### MassTransit Outbox Pattern
- Messages are persisted in PostgreSQL within the **same DB transaction** as domain writes.
- A background dispatcher forwards them to RabbitMQ once committed.
- Guarantees **at-least-once delivery** with idempotency on the consumer side.

### CQRS with MediatR
- Commands mutate state and publish integration events.
- Queries are read-only and use `AsNoTracking()` for performance.
- Pipeline behaviors add cross-cutting concerns (logging, validation) non-invasively.

### Clean Architecture Dependency Rule
```
API → Application → Domain   (depends inward)
Infrastructure → Application (implements interfaces)
```
The domain has zero external dependencies — pure C#.

### Scalability
- **Stateless** API pods — scale horizontally behind the ingress.
- **Consumer competition**: multiple NotificationService replicas consume from the same queue (RabbitMQ round-robin).
- **Connection pooling**: Npgsql pool with configurable min/max connections.
- **HPA**: CPU + memory based autoscaling with stabilization windows to prevent thrashing.

### Reliability
- **Retry policies**: Exponential back-off on both producer and consumer sides.
- **Kill switch**: MassTransit stops consuming if error rate exceeds 15%.
- **Health checks**: Liveness + readiness probes on all pods.
- **Zero-downtime deploys**: `maxUnavailable: 0` rolling update strategy.
- **Pod anti-affinity**: replicas spread across nodes.
- **Network policies**: Least-privilege pod communication.
