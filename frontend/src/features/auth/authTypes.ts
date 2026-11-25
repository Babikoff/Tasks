export type UserRole = 'Worker' | 'Operator' | 'Manager';

export interface IUser {
  username: string;
  email?: string;
  roles: UserRole[];
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn?: number;
}

export interface KeycloakTokenResponse {
  access_token: string;
  expires_in: number;
  refresh_expires_in: number;
  refresh_token: string;
  token_type: string;
  id_token?: string;
  'not-before-policy'?: number;
  session_state?: string;
  scope?: string;
}

export interface JwtPayload {
  exp: number;
  iat: number;
  auth_time?: number;
  jti?: string;
  iss?: string;
  sub: string;
  typ?: string;
  azp?: string;
  session_state?: string;
  preferred_username?: string;
  email?: string;
  email_verified?: boolean;
  name?: string;
  realm_access?: {
    roles: string[];
  };
  resource_access?: Record<string, { roles: string[] }>;
  roles?: string[];
}
