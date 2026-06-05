# Decomposed Java Microservices — Target State

This repository is the **target scaffolding** for decomposing the monolithic QuickApp application into cloud-native Java microservices deployed on Kubernetes.

## Source Monolith

The before-state monolith lives in [`app_dotnet_angular_containerized_decomposition_monolith`](https://github.com/Cognition-Partner-Workshops/app_dotnet_angular_containerized_decomposition_monolith).

## Architecture

The monolith's bounded contexts are decomposed into the following independently deployable microservices:

```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Identity    │  │  Customer    │  │   Order      │
│  Service     │  │  Service     │  │   Service    │
│  (Java 21)   │  │  (Java 21)   │  │  (Java 21)   │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       └────────────┬────┘─────────────────┘
                    │
              ┌─────┴──────┐
              │  API       │
              │  Gateway   │
              └─────┬──────┘
                    │
┌──────────────┐  ┌┴─────────────┐
│  Product     │  │ Notification │
│  Service     │  │  Service     │
│  (Java 21)   │  │  (Java 21)   │
└──────────────┘  └──────────────┘
```

## Services

| Service | Port | Description | Monolith Origin |
|---------|------|-------------|-----------------|
| `identity-service` | 5001 | Authentication, authorization, user/role management | `AuthorizationController`, `UserAccountController`, `UserRoleController` |
| `customer-service` | 5002 | Customer CRUD and lookup | `CustomerController`, customer models |
| `order-service` | 5003 | Order management and processing | `OrdersController`, order models |
| `product-service` | 5004 | Product catalog management | `ProductsController`, product models |
| `notification-service` | 5005 | Email and in-app notifications | `NotificationService`, notification models |
| `api-gateway` | 5000 | Spring Cloud Gateway reverse proxy, request routing | New — replaces monolith's single entry point |

## Project Structure

```
src/
├── pom.xml                            # Maven parent POM (Java 21, Spring Boot 3.3.x)
├── api-gateway/                       # Spring Cloud Gateway
│   ├── pom.xml
│   ├── src/main/java/.../ApiGatewayApplication.java
│   ├── src/main/resources/application.yml
│   └── Dockerfile
├── services/
│   ├── identity-service/
│   │   ├── pom.xml
│   │   ├── src/main/java/.../controller/IdentityController.java
│   │   ├── src/main/resources/application.yml
│   │   └── Dockerfile
│   ├── customer-service/
│   │   ├── ...
│   ├── order-service/
│   │   ├── ...
│   ├── product-service/
│   │   ├── ...
│   └── notification-service/
│       ├── pom.xml
│       ├── src/main/java/.../controller/NotificationController.java
│       ├── src/main/java/.../domain/entity/OrderNotification.java
│       ├── src/main/java/.../domain/repository/NotificationRepository.java
│       ├── src/main/java/.../service/OrderEventConsumer.java
│       ├── src/main/java/.../service/NotificationRenderer.java
│       ├── src/main/resources/application.yml
│       └── Dockerfile
├── shared/
│   ├── shared-contracts/              # Shared DTOs, events (Java records)
│   └── shared-infrastructure/         # Common middleware (CorrelationIdFilter), health checks
└── docker-compose.yml
```

## Technology Stack

- **Java 21** — Spring Boot 3.3.x Web API per service
- **Spring Data JPA** — per-service database (database-per-service pattern)
- **Spring Cloud Gateway** — API gateway / reverse proxy
- **SLF4J + Logback** — logging (built into Spring Boot)
- **SpringDoc OpenAPI** — API documentation (Swagger UI)
- **Spring Boot Actuator** — health checks at `/actuator/health`
- **RabbitMQ** — async messaging between services
- **Docker** — containerized services
- **Kubernetes** — orchestration (see `app_dotnet_angular_containerized_decomposition_iac` for Helm charts)

## Getting Started

Each service can be run independently:

```bash
# Run all services with Docker Compose
cd src && docker compose up --build

# Run a single service (requires Maven and Java 21)
cd src
mvn -pl services/identity-service -am spring-boot:run
```

## Health Checks

All services expose health check endpoints at `/actuator/health` (provided by Spring Boot Actuator).

## Related Repositories

| Repo | Purpose |
|------|---------|
| [`app_dotnet_angular_containerized_decomposition_monolith`](https://github.com/Cognition-Partner-Workshops/app_dotnet_angular_containerized_decomposition_monolith) | Before-state monolith |
| [`app_dotnet_angular_containerized_decomposition_microfrontends`](https://github.com/Cognition-Partner-Workshops/app_dotnet_angular_containerized_decomposition_microfrontends) | Angular micro-frontends target |
| [`app_dotnet_angular_containerized_decomposition_iac`](https://github.com/Cognition-Partner-Workshops/app_dotnet_angular_containerized_decomposition_iac) | App-specific Helm charts |
| [`platform-engineering-shared-services`](https://github.com/Cognition-Partner-Workshops/platform-engineering-shared-services) | Shared EKS cluster and platform infra |
