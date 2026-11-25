import { fetchBaseQuery, type BaseQueryFn, type FetchArgs, type FetchBaseQueryError } from '@reduxjs/toolkit/query/react';
import { setCredentials, logout } from '@/features/auth/authSlice';
import { tokenStorage } from '@/features/auth/tokenStorage';
import type { KeycloakTokenResponse } from '@/features/auth/authTypes';

const rawBaseQuery = fetchBaseQuery({
  baseUrl: '/',
  prepareHeaders: (headers) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    return headers;
  },
});

export const baseQueryWithTokenRefresh: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  let result = await rawBaseQuery(args, api, extraOptions);

  if (result.error && (result.error.status === 401 || result.error.status === 403)) {
    const refreshToken = tokenStorage.getRefreshToken();
    if (refreshToken) {
      // Attempt silent refresh via Keycloak token endpoint
      const refreshParams = new URLSearchParams();
      refreshParams.append('grant_type', 'refresh_token');
      refreshParams.append('client_id', 'tasksservice');
      refreshParams.append('refresh_token', refreshToken);

      const refreshResult = await rawBaseQuery(
        {
          url: '/realms/tasks-realm/protocol/openid-connect/token',
          method: 'POST',
          body: refreshParams.toString(),
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
        },
        api,
        extraOptions
      );

      if (refreshResult.data) {
        const tokenData = refreshResult.data as KeycloakTokenResponse;
        api.dispatch(
          setCredentials({
            accessToken: tokenData.access_token,
            refreshToken: tokenData.refresh_token,
          })
        );
        // Retry the original query with the new token
        result = await rawBaseQuery(args, api, extraOptions);
      } else {
        // Refresh token expired or invalid
        api.dispatch(logout());
      }
    } else {
      api.dispatch(logout());
    }
  }

  return result;
};
