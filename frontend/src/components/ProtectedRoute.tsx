import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '@/hooks/hooks';
import { selectCurrentUser, selectIsAuthenticated } from '@/features/auth/authSlice';
import type { UserRole } from '@/features/auth/authTypes';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: UserRole[];
}

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const user = useAppSelector(selectCurrentUser);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && user) {
    const hasRole = allowedRoles.some((role) => user.roles.includes(role));
    if (!hasRole) {
      return (
        <div className="flex flex-col items-center justify-center p-8 text-center">
          <h2 className="text-2xl font-bold text-red-600 mb-2">Access Denied</h2>
          <p className="text-slate-600 mb-4">
            Your role(s) [{user.roles.join(', ')}] do not have permission to view this page.
          </p>
        </div>
      );
    }
  }

  return <>{children}</>;
}
