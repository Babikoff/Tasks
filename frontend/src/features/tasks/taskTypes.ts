export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
}

export interface TaskItem {
  id: string;
  title: string;
  isCompleted: boolean;
  priority: Priority;
  createdAt: string;
  completedAt?: string | null;
}

export interface CreateTaskRequest {
  title: string;
  isCompleted: boolean;
  priority: Priority;
}

export interface CreateTaskResponse {
  id: string;
  title: string;
  isCompleted: boolean;
  priority: Priority;
  url: string;
}
