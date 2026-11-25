import { jwtDecode } from 'jwt-decode';
import type { IUser, JwtPayload, UserRole } from './authTypes';

const ACCESS_TOKEN_KEY = 'tasks_access_token';
const REFRESH_TOKEN_KEY = 'tasks_refresh_token';

export const tokenStorage = {
  getAccessToken: (): string | null => localStorage.getItem(ACCESS_TOKEN_KEY),
  getRefreshToken: (): string | null => localStorage.getItem(REFRESH_TOKEN_KEY),
  setTokens: (accessToken: string, refreshToken: string): void => {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clearTokens: (): void => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  },
};

export function parseUserFromToken(token: string): IUser | null {
  try {
    const payload = jwtDecode<JwtPayload>(token);

    // Check expiration
    if (payload.exp && Date.now() >= payload.exp * 1000) {
      return null;
    }

    const rawRoles: string[] = [
      ...(payload.realm_access?.roles ?? []),
      ...(payload.roles ?? []),
    ];

    const validRoles: UserRole[] = [];
    if (rawRoles.includes('Manager')) validRoles.push('Manager');
    if (rawRoles.includes('Operator')) validRoles.push('Operator');
    if (rawRoles.includes('Worker')) validRoles.push('Worker');

    return {
      username: payload.preferred_username || payload.name || payload.sub || 'User',
      email: payload.email,
      roles: validRoles,
    };
  } catch (error) {
    console.error('Failed to decode token:', error);
    return null;
  }
}
