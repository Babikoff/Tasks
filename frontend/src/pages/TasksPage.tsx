import { useState } from 'react';
import { useGetTasksQuery } from '@/features/tasks/tasksApi';
import { useAppSelector } from '@/hooks/hooks';
import { selectUserRoles } from '@/features/auth/authSlice';
import { TasksDataGrid } from '@/features/tasks/TasksDataGrid';
import { CreateTaskModal } from '@/features/tasks/CreateTaskModal';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Plus, RefreshCw, AlertCircle, ListTodo } from 'lucide-react';

export function TasksPage() {
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const { data: tasks = [], isLoading, isError, error, refetch, isFetching } = useGetTasksQuery();

  const roles = useAppSelector(selectUserRoles);
  const canCreate = roles.includes('Operator') || roles.includes('Manager');

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-900 flex items-center gap-2">
            <ListTodo className="h-7 w-7 text-blue-600" />
            <span>Tasks Management</span>
          </h1>
          <p className="text-sm text-slate-500 mt-1">
            View, sort, filter, and manage tasks according to your role permissions.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => refetch()}
            disabled={isFetching}
            className="gap-1.5"
            title="Refresh list"
          >
            <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin text-blue-600' : ''}`} />
            <span>Refresh</span>
          </Button>

          {canCreate && (
            <Button size="sm" onClick={() => setIsCreateOpen(true)} className="gap-1.5">
              <Plus className="h-4 w-4" />
              <span>Create Task</span>
            </Button>
          )}
        </div>
      </div>

      {/* Main Content Area */}
      {isLoading ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16 text-slate-500 space-y-3">
            <RefreshCw className="h-8 w-8 animate-spin text-blue-600" />
            <p className="text-sm font-medium">Loading tasks from backend...</p>
          </CardContent>
        </Card>
      ) : isError ? (
        <Card className="border-red-200 bg-red-50/50">
          <CardContent className="flex flex-col items-center justify-center py-12 text-center text-red-700 space-y-2">
            <AlertCircle className="h-8 w-8 text-red-600" />
            <h3 className="text-base font-semibold">Failed to load tasks</h3>
            <p className="text-xs text-red-600 max-w-md">
              {error && 'status' in error ? `Error status: ${error.status}` : 'Check your network connection or credentials.'}
            </p>
            <Button variant="outline" size="sm" onClick={() => refetch()} className="mt-2 bg-white">
              Try Again
            </Button>
          </CardContent>
        </Card>
      ) : (
        <TasksDataGrid tasks={tasks} />
      )}

      {/* Create Task Modal */}
      {canCreate && (
        <CreateTaskModal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} />
      )}
    </div>
  );
}
