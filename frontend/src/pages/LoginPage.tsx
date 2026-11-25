import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useLoginMutation } from '@/features/auth/authApi';
import { setCredentials } from '@/features/auth/authSlice';
import { useAppDispatch } from '@/hooks/hooks';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Lock, User, AlertCircle, ArrowRight, ShieldCheck } from 'lucide-react';

export function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [login, { isLoading }] = useLoginMutation();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/tasks';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!username.trim() || !password.trim()) {
      setErrorMessage('Please enter both username and password.');
      return;
    }

    try {
      const response = await login({ username, password }).unwrap();
      dispatch(
        setCredentials({
          accessToken: response.access_token,
          refreshToken: response.refresh_token,
        })
      );
      navigate(from, { replace: true });
    } catch {
      setErrorMessage('Invalid username or password. Please verify credentials.');
    }
  };

  const fill = (u: string, p: string) => {
    setUsername(u);
    setPassword(p);
    setErrorMessage(null);
  };

  return (
    <div className="flex min-h-[calc(100vh-8rem)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-md space-y-6">
        <Card className="shadow-lg border-slate-200">
          <CardHeader className="text-center space-y-2">
            <div className="mx-auto w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600">
              <Lock className="h-6 w-6" />
            </div>
            <CardTitle className="text-2xl font-bold">Sign In</CardTitle>
            <CardDescription>Enter Keycloak credentials to access tasks</CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit}>
            <CardContent className="space-y-4">
              {errorMessage && (
                <div className="flex items-center gap-2 p-3 text-sm text-red-700 bg-red-50 rounded-lg border border-red-200">
                  <AlertCircle className="h-4 w-4 shrink-0" />
                  <span>{errorMessage}</span>
                </div>
              )}
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Username</label>
                <div className="relative">
                  <User className="absolute left-3 top-2.5 h-4 w-4 text-slate-400" />
                  <Input type="text" placeholder="worker_user" className="pl-9" value={username} onChange={(e) => setUsername(e.target.value)} required />
                </div>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-700">Password</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-2.5 h-4 w-4 text-slate-400" />
                  <Input type="password" placeholder="••••••••" className="pl-9" value={password} onChange={(e) => setPassword(e.target.value)} required />
                </div>
              </div>
            </CardContent>
            <CardFooter>
              <Button type="submit" className="w-full gap-2" disabled={isLoading}>
                {isLoading ? 'Signing in...' : 'Sign In'}
                <ArrowRight className="h-4 w-4" />
              </Button>
            </CardFooter>
          </form>
        </Card>

        {/* Demo Credentials */}
        <Card className="bg-slate-50 border-dashed border-slate-300">
          <CardHeader className="pb-3 pt-4">
            <CardTitle className="text-xs uppercase tracking-wider text-slate-500 font-semibold flex items-center gap-1.5">
              <ShieldCheck className="h-4 w-4 text-slate-400" /> Demo Test Credentials
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 pb-4">
            <div onClick={() => fill('worker_user', 'worker123')} className="flex items-center justify-between p-2 rounded bg-white border border-slate-200 cursor-pointer hover:border-blue-400">
              <span className="text-xs font-semibold text-slate-800">worker_user</span>
              <Badge variant="success">Worker</Badge>
            </div>
            <div onClick={() => fill('operator_user', 'operator123')} className="flex items-center justify-between p-2 rounded bg-white border border-slate-200 cursor-pointer hover:border-blue-400">
              <span className="text-xs font-semibold text-slate-800">operator_user</span>
              <Badge variant="warning">Operator</Badge>
            </div>
            <div onClick={() => fill('manager_user', 'manager123')} className="flex items-center justify-between p-2 rounded bg-white border border-slate-200 cursor-pointer hover:border-blue-400">
              <span className="text-xs font-semibold text-slate-800">manager_user</span>
              <Badge variant="default">Manager</Badge>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
