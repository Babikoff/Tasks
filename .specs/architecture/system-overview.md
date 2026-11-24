# System Overview

## 1. Architecture Diagram & Services

The system is a microservice-oriented task management solution composed of:

- **Frontend (Web Client):** React (Vite, TypeScript) Single Page Application (SPA).
- **Reverse Proxy / Gateway:** Nginx handling SSL termination and path routing (`/` to Frontend, `/api/` to TasksService, `/realms/` to Keycloak).
- **Backend API:** `TasksService` (.NET 8 Minimal API, Entity Framework Core).
- **Identity Provider (IdP):** Keycloak for OIDC/OAuth2 authentication and role management.
- **Database:** PostgreSQL for `TasksService` and outbox tables.
- **Message Broker:** RabbitMQ for asynchronous domain event publishing via Transactional Outbox.

## 2. Ports and URLs

| Component | Internal (Docker) | Host Port | Path / Note |
| :--- | :--- | :--- | :--- |
| Nginx | 80 / 443 | 80 / 443 | Main Entrypoint |
| TasksService | 8080 | 8080 (dev) | REST API `/api/tasks` |
| Keycloak | 8080 | 8081 | Realm: `tasks-realm` |
| PostgreSQL | 5432 | 5432 | Database `tasksdb` |
| RabbitMQ | 5672 / 15672 | 5672 / 15672 | Management UI on 15672 |
| Frontend | 80 (Nginx) / 5173 (Vite dev) | - | React UI |

## 3. Communication Patterns
- **Synchronous:** HTTP/REST with JWT Bearer tokens from Frontend/Clients to `TasksService`.
- **Asynchronous:** Outbox pattern writing domain events to Postgres within the same transaction, processed and published to RabbitMQ queue `tasks-queue`.
