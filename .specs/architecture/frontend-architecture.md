# Frontend Architecture Specification

## 1. Core Technology Stack
- **Framework:** React 19 (functional components only)
- **Language:** TypeScript 5.x (strict type-checking)
- **Build Tool:** Vite 6/7 with `@vitejs/plugin-react` and `vite-tsconfig-paths`
- **Linting & Formatting:** ESLint 9 (Flat Config with `typescript-eslint`, `eslint-plugin-react-hooks`, `eslint-plugin-perfectionist`) and Prettier
- **Testing:** Vitest with jsdom and React Testing Library (`@testing-library/react`)

## 2. Styling & UI Components
- **Styling:** Tailwind CSS (v4 / v3 with PostCSS)
- **UI Components:** `shadcn/ui` (accessible components based on Radix UI primitives and Tailwind CSS)
- **Icons:** `lucide-react`
- **Data Grids & Visualizations:** Evaluation of `@tanstack/react-table` and `recharts` for complex datagrids and histograms.

## 3. State Management & Data Fetching
- **Redux Toolkit (RTK):** Application state management
- **RTK Query:** API communication layer (`createApi` with `fetchBaseQuery` / custom base query handling Bearer tokens)
- **Typed Hooks:** Pre-typed hooks `useAppDispatch` and `useAppSelector` matching patterns from `@react-example/src/hooks/hooks.ts`
- **Store Architecture:** Slices combined via `combineSlices` / `configureStore`

## 4. Routing & Authentication
- **Router:** React Router v7 (`react-router-dom`)
- **Route Guards:** `ProtectedRoute` component for RBAC and unauthenticated redirects
- **OIDC/Keycloak Auth:** OAuth2 Authorization Code flow with PKCE storing access and refresh tokens, attaching `Bearer <token>` to API requests

## 5. Project Structure & Conventions
```text
frontend/
├── src/
│   ├── components/         # Reusable UI & layout components (shadcn/ui, modals, guards)
│   ├── hooks/              # Custom React hooks (e.g. useAppDispatch, useFormWithValidation)
│   ├── pages/              # Page-level components
│   ├── services/           # RTK store, RTK Query API definitions, slices
│   ├── utils/              # Helper functions, constants, validators
│   ├── main.tsx            # Application entrypoint
│   └── App.tsx             # Root router & layout
├── index.html
├── vite.config.ts
├── tsconfig.json
├── eslint.config.js
└── package.json
```

## 6. Patterns Adopted from `@react-example`
- Typed hooks setup (`useAppDispatch`, `useAppSelector`).
- Form handling with validation (`useFormWithValidation`).
- Token handling and API request abstractions.
- Deterministic import sorting (via `eslint-plugin-perfectionist`).
