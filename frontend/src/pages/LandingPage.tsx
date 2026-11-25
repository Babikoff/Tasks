import { Link } from 'react-router-dom';
import { useGetHealthQuery } from '@/features/tasks/tasksApi';
import { useAppSelector } from '@/hooks/hooks';
import { selectIsAuthenticated } from '@/features/auth/authSlice';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { CheckCircle2, Shield, ArrowRight, Activity, Users, Zap } from 'lucide-react';

export function LandingPage() {
  const { data: health, isLoading: isHealthLoading, isError: isHealthError } = useGetHealthQuery();
  const isAuthenticated = useAppSelector(selectIsAuthenticated);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-12">
      {/* Hero Section */}
      <section className="text-center space-y-4 max-w-3xl mx-auto">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-blue-50 border border-blue-200 text-blue-700 text-xs font-semibold uppercase tracking-wider">
          <Zap className="h-3.5 w-3.5" /> Spec-Driven Architecture
        </div>
        <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight text-slate-900">
          Enterprise Task Management System
        </h1>
        <p className="text-lg text-slate-600">
          Powered by React 19, Redux Toolkit, .NET 8 Minimal APIs, PostgreSQL, Keycloak OIDC, and RabbitMQ Outbox.
        </p>

        {/* System Health Badge */}
        <div className="flex items-center justify-center gap-2 pt-2">
          <Activity className="h-4 w-4 text-slate-500" />
          <span className="text-sm font-medium text-slate-600">Tasks Backend Status:</span>
          {isHealthLoading ? (
            <Badge variant="outline">Checking...</Badge>
          ) : isHealthError ? (
            <Badge variant="destructive">Backend Offline</Badge>
          ) : (
            <Badge variant="success" className="gap-1">
              <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
              Healthy ({health || 'OK'})
            </Badge>
          )}
        </div>

        {/* Call to Actions */}
        <div className="flex justify-center gap-4 pt-4">
          {isAuthenticated ? (
            <Link to="/tasks">
              <Button size="lg" className="gap-2">
                <span>Go to Tasks DataGrid</span>
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>
          ) : (
            <Link to="/login">
              <Button size="lg" className="gap-2">
                <span>Sign In to Access Tasks</span>
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>
          )}
        </div>
      </section>

      {/* Feature / Role Overview Cards */}
      <section className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="hover:shadow-md transition-shadow">
          <CardHeader>
            <div className="h-10 w-10 rounded-lg bg-blue-100 flex items-center justify-center text-blue-600 mb-2">
              <Users className="h-5 w-5" />
            </div>
            <CardTitle>Worker Role</CardTitle>
            <CardDescription>Execution and completion of tasks</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm text-slate-600">
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              View assigned tasks in the DataGrid
            </p>
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              Mark tasks as completed (triggers Outbox event)
            </p>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader>
            <div className="h-10 w-10 rounded-lg bg-amber-100 flex items-center justify-center text-amber-600 mb-2">
              <Zap className="h-5 w-5" />
            </div>
            <CardTitle>Operator Role</CardTitle>
            <CardDescription>Creation and queueing of tasks</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm text-slate-600">
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              View all current tasks
            </p>
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              Create new tasks with priorities (Low, Medium, High)
            </p>
          </CardContent>
        </Card>

        <Card className="hover:shadow-md transition-shadow">
          <CardHeader>
            <div className="h-10 w-10 rounded-lg bg-emerald-100 flex items-center justify-center text-emerald-600 mb-2">
              <Shield className="h-5 w-5" />
            </div>
            <CardTitle>Manager Role</CardTitle>
            <CardDescription>Full administrative control</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2 text-sm text-slate-600">
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              Full CRUD access to all task operations
            </p>
            <p className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-500 shrink-0" />
              Complete and delete tasks with instant updates
            </p>
          </CardContent>
        </Card>
      </section>
    </div>
  );
}
