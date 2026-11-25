import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/hooks/hooks';
import { logout, selectCurrentUser, selectIsAuthenticated } from '@/features/auth/authSlice';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Sheet } from '@/components/ui/Sheet';
import { CheckSquare, LogIn, LogOut, Menu, User } from 'lucide-react';

export function Navbar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const user = useAppSelector(selectCurrentUser);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/');
    setIsMobileMenuOpen(false);
  };

  const isActive = (path: string) => location.pathname === path;

  return (
    <header className="sticky top-0 z-40 w-full border-b border-slate-200 bg-white/95 backdrop-blur-xs shadow-xs">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex h-16 items-center justify-between">
        <div className="flex items-center gap-6">
          <Link to="/" className="flex items-center gap-2 text-blue-600 font-bold text-xl">
            <CheckSquare className="h-6 w-6" />
            <span>Tasks App</span>
          </Link>
          <nav className="hidden md:flex items-center gap-4 text-sm font-medium">
            <Link to="/" className={`hover:text-blue-600 ${isActive('/') ? 'text-blue-600 font-semibold' : 'text-slate-600'}`}>
              Overview
            </Link>
            {isAuthenticated && (
              <Link to="/tasks" className={`hover:text-blue-600 ${isActive('/tasks') ? 'text-blue-600 font-semibold' : 'text-slate-600'}`}>
                Tasks
              </Link>
            )}
          </nav>
        </div>

        <div className="hidden md:flex items-center gap-4">
          {isAuthenticated && user ? (
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 text-sm bg-slate-100 px-3 py-1.5 rounded-full border border-slate-200">
                <User className="h-4 w-4 text-slate-500" />
                <span className="font-medium text-slate-800">{user.username}</span>
                <div className="flex gap-1">
                  {user.roles.map((r) => (
                    <Badge key={r} variant={r === 'Manager' ? 'default' : r === 'Operator' ? 'warning' : 'success'}>
                      {r}
                    </Badge>
                  ))}
                </div>
              </div>
              <Button variant="outline" size="sm" onClick={handleLogout} className="gap-1.5">
                <LogOut className="h-4 w-4" />
                <span>Logout</span>
              </Button>
            </div>
          ) : (
            <Link to="/login">
              <Button size="sm" className="gap-1.5">
                <LogIn className="h-4 w-4" />
                <span>Sign In</span>
              </Button>
            </Link>
          )}
        </div>

        <div className="flex md:hidden">
          <button onClick={() => setIsMobileMenuOpen(true)} className="p-2 text-slate-600 hover:text-slate-900" aria-label="Toggle menu">
            <Menu className="h-6 w-6" />
          </button>
        </div>
      </div>

      <Sheet isOpen={isMobileMenuOpen} onClose={() => setIsMobileMenuOpen(false)}>
        <div className="flex flex-col h-full justify-between">
          <div className="space-y-6 pt-4">
            <div className="flex items-center gap-2 text-blue-600 font-bold text-lg">
              <CheckSquare className="h-5 w-5" />
              <span>Tasks App</span>
            </div>
            {isAuthenticated && user && (
              <div className="p-3 bg-slate-50 rounded-lg border border-slate-200 space-y-2">
                <div className="flex items-center gap-2 text-sm font-medium text-slate-800">
                  <User className="h-4 w-4 text-slate-500" />
                  <span>{user.username}</span>
                </div>
                <div className="flex flex-wrap gap-1">
                  {user.roles.map((r) => (
                    <Badge key={r} variant={r === 'Manager' ? 'default' : r === 'Operator' ? 'warning' : 'success'}>
                      {r}
                    </Badge>
                  ))}
                </div>
              </div>
            )}
            <nav className="flex flex-col space-y-2">
              <Link to="/" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-md text-sm font-medium text-slate-700 hover:bg-slate-100">
                Overview
              </Link>
              {isAuthenticated && (
                <Link to="/tasks" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-md text-sm font-medium text-slate-700 hover:bg-slate-100">
                  Tasks Management
                </Link>
              )}
            </nav>
          </div>
          <div className="pt-6 border-t border-slate-200">
            {isAuthenticated ? (
              <Button variant="outline" className="w-full justify-center gap-2" onClick={handleLogout}>
                <LogOut className="h-4 w-4" />
                <span>Logout</span>
              </Button>
            ) : (
              <Link to="/login" onClick={() => setIsMobileMenuOpen(false)}>
                <Button className="w-full justify-center gap-2">
                  <LogIn className="h-4 w-4" />
                  <span>Sign In</span>
                </Button>
              </Link>
            )}
          </div>
        </div>
      </Sheet>
    </header>
  );
}
