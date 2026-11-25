import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { IUser } from './authTypes';
import { parseUserFromToken, tokenStorage } from './tokenStorage';

interface AuthState {
  user: IUser | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
}

const initialToken = tokenStorage.getAccessToken();
const initialUser = initialToken ? parseUserFromToken(initialToken) : null;

const initialState: AuthState = {
  user: initialUser,
  isAuthenticated: !!initialUser,
  isInitialized: false,
};

export const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ accessToken: string; refreshToken: string }>
    ) => {
      const { accessToken, refreshToken } = action.payload;
      tokenStorage.setTokens(accessToken, refreshToken);
      const user = parseUserFromToken(accessToken);
      state.user = user;
      state.isAuthenticated = !!user;
      state.isInitialized = true;
    },
    logout: (state) => {
      tokenStorage.clearTokens();
      state.user = null;
      state.isAuthenticated = false;
      state.isInitialized = true;
    },
    setInitialized: (state) => {
      state.isInitialized = true;
    },
  },
});

export const { setCredentials, logout, setInitialized } = authSlice.actions;

export const selectCurrentUser = (state: { auth: AuthState }) => state.auth.user;
export const selectIsAuthenticated = (state: { auth: AuthState }) => state.auth.isAuthenticated;
export const selectIsInitialized = (state: { auth: AuthState }) => state.auth.isInitialized;
export const selectUserRoles = (state: { auth: AuthState }) => state.auth.user?.roles ?? [];

export default authSlice.reducer;
