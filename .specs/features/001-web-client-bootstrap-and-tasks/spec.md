# Feature Specification: Web Client Bootstrap, Authentication & Tasks Management

## 1. Overview & Goal
Build the initial React 19 Single Page Application (SPA) with Vite, Tailwind CSS, shadcn/ui, and Redux Toolkit / RTK Query.
The web client must provide:
- A public landing page displaying system information and service health before authentication.
- Keycloak-based authentication with reliable JWT storage and automatic token refreshing.
- A responsive, mobile-first navigation shell.
- A task management view with a DataGrid for viewing, sorting, and filtering tasks, with role-based actions (Create, Complete, Delete).

## 2. User Stories
- **US-1 (Public Landing):** As an unauthenticated user, I want to see a welcoming landing page with system status and instructions to log in.
- **US-2 (Authentication):** As a registered user (Worker, Operator, Manager), I want to log in using my credentials, see my current role in the header, and be able to log out.
- **US-3 (Token Refresh):** As an active user, I want my session to refresh automatically in the background without interrupting my work when the access token expires.
- **US-4 (Task Viewing & DataGrid):** As an authorized user, I want to view a table of tasks with sorting and filtering by title.
- **US-5 (Role-Based Task Operations):**
  - As an Operator or Manager, I want to create new tasks with priority and title.
  - As a Worker or Manager, I want to mark tasks as completed.
  - As a Manager, I want to delete tasks.
- **US-6 (Mobile Responsiveness):** As a mobile user, I want a responsive layout with a collapsible navigation menu and an adaptive data grid.

## 3. Functional Requirements
- **FR-1:** Display public health status fetched from `/health` on the landing page.
- **FR-2:** Authenticate against Keycloak via Direct Access Grants / Token endpoint (`tasks-realm`, client `tasksservice`).
- **FR-3:** Store tokens and decode JWT payload to extract user identity and roles (`Worker`, `Operator`, `Manager`).
- **FR-4:** RTK Query `baseQueryWithTokenRefresh` must silently refresh the access token on `401/403` responses using the refresh token.
- **FR-5:** Render tasks list in a data grid with columns: Title, Priority, Status, Created At, Completed At, and Actions.
- **FR-6:** Provide client-side sorting (Title, Priority, Date) and search filtering (by Title).
- **FR-7:** Conditionally render action buttons (Create Task, Complete Task, Delete Task) based on current user roles.
- **FR-8:** Responsive navigation bar with mobile hamburger menu drawer.

## 4. Non-Functional Requirements
- **Modern Stack:** React 19 functional components, Vite, TypeScript, Tailwind CSS, shadcn/ui.
- **Microfrontend Readiness:** Slices organized under modular feature directories (`features/auth`, `features/tasks`).
- **Design:** Mobile-first layout with smooth transitions and clear loading/error feedback.

## 5. Acceptance Criteria
- [ ] Landing page renders with system info and `/health` indicator without requiring login.
- [ ] User can log in with valid credentials (`worker_user`, `operator_user`, `manager_user`); invalid credentials show clear error message.
- [ ] Logged-in user sees their username and role badge in the navbar.
- [ ] Authenticated API requests carry `Authorization: Bearer <access_token>`.
- [ ] On token expiration, RTK Query automatically fetches a new access token via refresh token without failing the user request.
- [ ] Tasks table displays all tasks fetched from `GET /api/tasks`.
- [ ] Task sorting and text search work correctly.
- [ ] Role permissions match the RBAC policy (Operator creates, Worker completes, Manager has all actions).
- [ ] Navigation folds into a mobile drawer on screens `< 768px`.
