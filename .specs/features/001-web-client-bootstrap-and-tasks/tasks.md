# Tasks Checklist: Web Client Bootstrap, Authentication & Tasks Management

## Phase 1: Project Scaffolding & Setup
- [x] Step 1: Initialize Vite + React 19 + TypeScript project in `frontend/` directory.
- [x] Step 2: Configure Tailwind CSS, PostCSS, and path aliases in `vite.config.ts` & `tsconfig.json`.
- [x] Step 3: Install & configure ESLint 9 (Flat Config), Prettier, and Lucide icons.
- [x] Step 4: Add base shadcn/ui primitives (`Button`, `Card`, `Input`, `Badge`, `Dialog`, `Table`, `Sheet`).

## Phase 2: State Management & Authentication
- [x] Step 5: Implement Redux store setup (`store.ts`, `hooks.ts` with `useAppDispatch`, `useAppSelector`).
- [x] Step 6: Create Keycloak auth slice & token management with JWT decoding and localStorage storage.
- [x] Step 7: Implement RTK Query `baseQueryWithTokenRefresh` with silent token refresh on 401/403.
- [x] Step 8: Build `ProtectedRoute` component for route security.

## Phase 3: Layout, Navigation & Landing Page
- [x] Step 9: Create responsive `Navbar` with desktop links and mobile hamburger sheet drawer.
- [x] Step 10: Build public `LandingPage` with system overview, `/health` status badge, and call-to-action.
- [x] Step 11: Build `LoginPage` with credential inputs, error handling banner, and test user quick-fill shortcuts.

## Phase 4: Tasks RTK Query & DataGrid
- [x] Step 12: Implement `tasksApi` with endpoints (`getTasks`, `createTask`, `completeTask`, `deleteTask`).
- [x] Step 13: Build `TasksPage` with search/filter bar and priority badges.
- [x] Step 14: Build `TasksDataGrid` with client-side sorting by Title, Priority, and Date.
- [x] Step 15: Implement `CreateTaskModal` form for Operator and Manager roles.
- [x] Step 16: Connect role-based action buttons (Complete for Worker/Manager, Delete for Manager).

## Phase 5: Verification & Containerization
- [x] Step 17: Update `docker-compose.yml` and `nginx/nginx.conf` to serve frontend.
- [x] Step 18: Verify all acceptance criteria across mobile and desktop viewport sizes.

