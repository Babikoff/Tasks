import * as React from 'react';
import { cn } from '@/utils/cn';
import { X } from 'lucide-react';

export interface SheetProps {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
}

export function Sheet({ isOpen, onClose, children }: SheetProps) {
  React.useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex">
      <div
        className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs transition-opacity"
        onClick={onClose}
      />
      <div
        className={cn(
          'relative ml-auto flex h-full w-3/4 max-w-sm flex-col bg-white p-6 shadow-2xl transition ease-in-out duration-300'
        )}
      >
        <button
          onClick={onClose}
          className="absolute right-4 top-4 rounded-sm opacity-70 hover:opacity-100 focus:outline-none"
        >
          <X className="h-5 w-5 text-slate-500" />
        </button>
        {children}
      </div>
    </div>
  );
}
