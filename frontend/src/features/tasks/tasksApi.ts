import { createApi } from '@reduxjs/toolkit/query/react';
import { baseQueryWithTokenRefresh } from '@/services/baseQueryWithTokenRefresh';
import type { CreateTaskRequest, CreateTaskResponse, TaskItem } from './taskTypes';

export const tasksApi = createApi({
  reducerPath: 'tasksApi',
  baseQuery: baseQueryWithTokenRefresh,
  tagTypes: ['Tasks'],
  endpoints: (builder) => ({
    getTasks: builder.query<TaskItem[], void>({
      query: () => 'api/tasks',
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Tasks' as const, id })), { type: 'Tasks', id: 'LIST' }]
          : [{ type: 'Tasks', id: 'LIST' }],
    }),
    getTaskById: builder.query<TaskItem, string>({
      query: (id) => `api/tasks/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Tasks', id }],
    }),
    createTask: builder.mutation<CreateTaskResponse, CreateTaskRequest>({
      query: (newTask) => ({
        url: 'api/tasks',
        method: 'POST',
        body: newTask,
      }),
      invalidatesTags: [{ type: 'Tasks', id: 'LIST' }],
    }),
    completeTask: builder.mutation<boolean, string>({
      query: (id) => ({
        url: `api/tasks/${id}/complete`,
        method: 'PUT',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'Tasks', id }, { type: 'Tasks', id: 'LIST' }],
    }),
    deleteTask: builder.mutation<boolean, string>({
      query: (id) => ({
        url: `api/tasks/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'Tasks', id }, { type: 'Tasks', id: 'LIST' }],
    }),
    getHealth: builder.query<string, void>({
      query: () => ({
        url: 'health',
        responseHandler: (response) => response.text(),
      }),
    }),
  }),
});

export const {
  useGetTasksQuery,
  useGetTaskByIdQuery,
  useCreateTaskMutation,
  useCompleteTaskMutation,
  useDeleteTaskMutation,
  useGetHealthQuery,
} = tasksApi;
