# Authentication and Authorization Flow

## 1. Identity Provider
- **Provider:** Keycloak
- **Realm:** `tasks-realm`
- **Frontend Client:** `tasks-web` (Public client, PKCE enabled, standard authorization code flow)
- **Backend API Audience:** `tasksservice`

## 2. Authentication Flow (Frontend -> Keycloak)
1. User opens the React Web Client.
2. Web client checks for an active session or stored OIDC tokens.
3. If not authenticated, redirect to Keycloak login page using OAuth2 Authorization Code Flow with PKCE.
4. After successful login, Keycloak redirects back to the Web Client with an authorization code.
5. Web Client exchanges the code for `access_token`, `refresh_token`, and `id_token`.
6. Web Client attaches the `access_token` in the `Authorization: Bearer <token>` header on all requests to `TasksService`.

## 3. Role-Based Access Control (RBAC)

The system defines three primary roles:

| Role | Permissions / Endpoints |
| :--- | :--- |
| **Worker** | `GET /api/tasks`, `GET /api/tasks/{id}`, `PUT /api/tasks/{id}/complete` |
| **Operator** | `GET /api/tasks`, `GET /api/tasks/{id}`, `POST /api/tasks` |
| **Manager** | Full access (`GET`, `POST`, `PUT /complete`, `DELETE /api/tasks/{id}`) |

Keycloak token includes roles in `realm_access.roles` and `roles` claim.
`TasksService` validates these roles using .NET Authorization policies (`CanViewTasks`, `CanCreateTasks`, `CanCompleteTasks`, `CanDeleteTasks`).
