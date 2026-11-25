import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { KeycloakTokenResponse } from './authTypes';

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/' }),
  endpoints: (builder) => ({
    login: builder.mutation<KeycloakTokenResponse, { username: string; password: string }>({
      query: (credentials) => {
        const body = new URLSearchParams();
        body.append('grant_type', 'password');
        body.append('client_id', 'tasksservice');
        body.append('username', credentials.username);
        body.append('password', credentials.password);

        return {
          url: 'realms/tasks-realm/protocol/openid-connect/token',
          method: 'POST',
          body: body.toString(),
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
        };
      },
    }),
  }),
});

export const { useLoginMutation } = authApi;
