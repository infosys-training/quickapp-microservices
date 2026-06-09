# Docker Orchestration & Networking

## Architecture Overview

This stack uses **Nginx** as a reverse proxy entry point, routing traffic to the YARP-based API Gateway and individual microservices. All services are orchestrated via Docker Compose with health checks and proper startup ordering.

```
                    ┌──────────────┐
                    │    Nginx     │  :80
                    │ (reverse     │
                    │  proxy)      │
                    └──────┬───────┘
                           │
              ┌────────────┴────────────┐
              │      frontend network    │
              └────────────┬────────────┘
                           │
                    ┌──────┴───────┐
                    │  API Gateway │  :5000 (YARP)
                    └──────┬───────┘
                           │
         ┌─────────────────┼─────────────────┐
         │         backend network            │
         │                                    │
    ┌────┴────┐  ┌────────┐  ┌────────┐  ┌──┴─────┐  ┌────────────┐
    │Identity │  │Customer│  │ Order  │  │Product │  │Notification│
    │ :5001   │  │ :5002  │  │ :5003  │  │ :5004  │  │   :5005    │
    └────┬────┘  └───┬────┘  └───┬────┘  └───┬────┘  └─────┬──────┘
         │           │           │           │              │
         └───────────┴───────────┴───────────┴──────────────┘
                           │
              ┌────────────┴────────────┐
              │    PostgreSQL  :5432     │
              │    RabbitMQ   :5672      │
              └─────────────────────────┘
```

## Networks

| Network    | Purpose                                | Services                                         |
|------------|----------------------------------------|--------------------------------------------------|
| `frontend` | Public-facing traffic                  | nginx, api-gateway                               |
| `backend`  | Internal service mesh                  | All application services, postgres, rabbitmq, api-gateway, nginx |

The API Gateway bridges both networks — it receives traffic from Nginx on the `frontend` network and routes to backend services on the `backend` network.

## Service Dependencies & Startup Order

Services start in the following order enforced by `depends_on` with `condition: service_healthy`:

```
1. postgres          (no dependencies)
2. rabbitmq          (no dependencies)
3. identity-service  (depends on: postgres ✓, rabbitmq ✓)
4. customer-service  (depends on: postgres ✓, rabbitmq ✓)
5. order-service     (depends on: postgres ✓, rabbitmq ✓)
6. product-service   (depends on: postgres ✓, rabbitmq ✓)
7. notification-service (depends on: postgres ✓, rabbitmq ✓)
8. api-gateway       (depends on: all 5 application services ✓)
9. nginx             (depends on: api-gateway ✓)
```

### Dependency Rationale

- **postgres**: All services use database-per-service pattern with PostgreSQL as the backing store. Each creates its own database (`identitydb`, `customerdb`, etc.).
- **rabbitmq**: Used for async inter-service messaging (e.g., `OrderPlacedEvent` triggers notification generation).
- **api-gateway → services**: YARP needs backends available for routing.
- **nginx → api-gateway**: Entry point requires gateway to be responsive.

## Health Checks

| Service              | Endpoint                              | Start Period |
|----------------------|---------------------------------------|--------------|
| postgres             | `pg_isready -U postgres`              | 10s          |
| rabbitmq             | `rabbitmq-diagnostics -q ping`        | 30s          |
| identity-service     | `GET http://localhost:5001/healthz`    | 30s          |
| customer-service     | `GET http://localhost:5002/healthz`    | 30s          |
| order-service        | `GET http://localhost:5003/healthz`    | 30s          |
| product-service      | `GET http://localhost:5004/healthz`    | 30s          |
| notification-service | `GET http://localhost:5005/healthz`    | 30s          |
| api-gateway          | `GET http://localhost:5000/healthz`    | 30s          |
| nginx                | `GET http://localhost/nginx-health`    | 10s          |

## Endpoints via Reverse Proxy (port 80)

| Path                 | Routed To                          | Description                 |
|----------------------|------------------------------------|-----------------------------|
| `/`                  | api-gateway:5000                   | All API routes via YARP     |
| `/api/identity/*`    | api-gateway → identity-service     | Auth & user management      |
| `/api/customers/*`   | api-gateway → customer-service     | Customer CRUD               |
| `/api/orders/*`      | api-gateway → order-service        | Order management            |
| `/api/products/*`    | api-gateway → product-service      | Product catalog             |
| `/api/notifications/*` | api-gateway → notification-service | Notifications             |
| `/identity/`         | identity-service:5001 (direct)     | Direct service access       |
| `/customers/`        | customer-service:5002 (direct)     | Direct service access       |
| `/orders/`           | order-service:5003 (direct)        | Direct service access       |
| `/products/`         | product-service:5004 (direct)      | Direct service access       |
| `/notifications/`    | notification-service:5005 (direct) | Direct service access       |
| `/rabbitmq/`         | rabbitmq:15672                     | RabbitMQ Management UI      |
| `/nginx-health`      | nginx (local)                      | Nginx health check          |

## Running the Stack

```bash
# Start everything
docker compose up --build

# Verify all containers are healthy
docker compose ps

# Check a specific service health
curl http://localhost/nginx-health
curl http://localhost:5000/healthz

# View logs for a specific service
docker compose logs -f api-gateway

# Shut down
docker compose down
```
