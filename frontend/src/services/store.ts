import { configureStore } from '@reduxjs/toolkit';
import authReducer from '@/features/auth/authSlice';
import { authApi } from '@/features/auth/authApi';
import { tasksApi } from '@/features/tasks/tasksApi';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    [authApi.reducerPath]: authApi.reducer,
    [tasksApi.reducerPath]: tasksApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(authApi.middleware, tasksApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
