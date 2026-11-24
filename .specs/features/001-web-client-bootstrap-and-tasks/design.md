# Technical Design: Web Client Bootstrap, Authentication & Tasks Management

## 1. Architecture & Component Hierarchy

### Application Tree
```text
App (Router & Theme Provider)
├── Navbar (Header, Brand, Navigation Links, Auth Controls, Mobile Drawer)
├── MainContent (Routes)
│   ├── LandingPage (Public info, Service Health Badge, Login Call to Action)
│   ├── LoginPage (Card / Modal form, credential inputs, test user quick-fill)
│   └── TasksPage (Protected)
│       ├── TasksHeader (Title, New Task Button for Operator/Manager, Search Input)
│       ├── TasksDataGrid (Responsive Table / Cards, Sorting, Status/Priority Badges)
│       └── CreateTaskModal (Form with Title, Priority select, submit/cancel)
└── Footer
```

## 2. Authentication & Token Management Strategy

### Keycloak OIDC Integration
- **Token Endpoint:** `POST /realms/tasks-realm/protocol/openid-connect/token`
- **Login Request:** `grant_type=password`, `client_id=tasksservice`, `username`, `password`
- **Refresh Request:** `grant_type=refresh_token`, `client_id=tasksservice`, `refresh_token`
- **JWT Parsing:** Parse `access_token` payload to extract `preferred_username`, `email`, and `realm_access.roles` (or `roles` claim).

### RTK Query Interceptor (`baseQueryWithTokenRefresh`)
- Modeled after `@react-example/src/services/api.ts`.
- When an API call returns `401 Unauthorized` or `403 Forbidden`, the base query attempts to refresh the access token via `refreshToken()`.
- If refresh succeeds, the original query is retried with the new bearer token.
- If refresh fails, tokens are cleared and the user state is reset to unauthenticated.

## 3. State Management & API Slices

### Store Layout (`@services/store.ts`)
- **`authSlice`**: Stores `{ user: IUser | null, isAuthenticated: boolean, roles: string[] }`.
- **`authApi`**: RTK Query endpoints for `login`, `refreshToken`, and Keycloak interactions.
- **`tasksApi`**: RTK Query endpoints for tasks CRUD:
  - `getTasks`: `GET /api/tasks` (providesTags: `['Tasks']`)
  - `getTaskById`: `GET /api/tasks/{id}`
  - `createTask`: `POST /api/tasks` (invalidatesTags: `['Tasks']`)
  - `completeTask`: `PUT /api/tasks/{id}/complete` (invalidatesTags: `['Tasks']`)
  - `deleteTask`: `DELETE /api/tasks/{id}` (invalidatesTags: `['Tasks']`)
  - `getHealth`: `GET /health` (public)

## 4. UI & Styling Framework
- **Tailwind CSS v4 / v3** with utility classes.
- **shadcn/ui Components:**
  - `Button`, `Card`, `Input`, `Badge`, `Dialog / Modal`, `Table`, `Sheet` (for mobile drawer).
- **Responsive Layout:**
  - Mobile (`< 768px`): Header collapses into a hamburger icon with a slide-out drawer; tasks render as cards or horizontally scrollable table.
  - Desktop (`>= 768px`): Full navbar with data grid table.

## 5. Security & RBAC Guards
- `ProtectedRoute` component wrapping `/tasks` to enforce authentication.
- Component-level authorization checks:
  - `hasRole('Operator') || hasRole('Manager')` for Task Creation.
  - `hasRole('Worker') || hasRole('Manager')` for Task Completion.
  - `hasRole('Manager')` for Task Deletion.
