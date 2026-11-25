import { useState, useMemo } from 'react';
import type { TaskItem } from '@/features/tasks/taskTypes';
import { Priority } from '@/features/tasks/taskTypes';
import { useCompleteTaskMutation, useDeleteTaskMutation } from '@/features/tasks/tasksApi';
import { useAppSelector } from '@/hooks/hooks';
import { selectUserRoles } from '@/features/auth/authSlice';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { CheckCircle2, Trash2, ArrowUpDown, Search } from 'lucide-react';

interface TasksDataGridProps {
  tasks: TaskItem[];
}

type SortField = 'title' | 'priority' | 'createdAt' | 'isCompleted';

export function TasksDataGrid({ tasks }: TasksDataGridProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortAsc, setSortAsc] = useState(false);

  const roles = useAppSelector(selectUserRoles);
  const canComplete = roles.includes('Worker') || roles.includes('Manager');
  const canDelete = roles.includes('Manager');

  const [completeTask, { isLoading: isCompleting }] = useCompleteTaskMutation();
  const [deleteTask, { isLoading: isDeleting }] = useDeleteTaskMutation();

  const handleSort = (field: SortField) => {
    if (sortField === field) setSortAsc(!sortAsc);
    else {
      setSortField(field);
      setSortAsc(true);
    }
  };

  const filteredTasks = useMemo(() => {
    return tasks
      .filter((t) => t.title.toLowerCase().includes(searchTerm.toLowerCase().trim()))
      .sort((a, b) => {
        let cmp = 0;
        if (sortField === 'title') cmp = a.title.localeCompare(b.title);
        else if (sortField === 'priority') cmp = a.priority - b.priority;
        else if (sortField === 'isCompleted') cmp = (a.isCompleted ? 1 : 0) - (b.isCompleted ? 1 : 0);
        else if (sortField === 'createdAt') cmp = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        return sortAsc ? cmp : -cmp;
      });
  }, [tasks, searchTerm, sortField, sortAsc]);

  const getPriorityBadge = (p: Priority) => {
    if (p === Priority.High) return <Badge variant="destructive">High</Badge>;
    if (p === Priority.Medium) return <Badge variant="warning">Medium</Badge>;
    return <Badge variant="secondary">Low</Badge>;
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2 max-w-sm">
        <div className="relative w-full">
          <Search className="absolute left-3 top-2.5 h-4 w-4 text-slate-400" />
          <Input placeholder="Search tasks..." className="pl-9" value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} />
        </div>
      </div>

      <div className="rounded-lg border border-slate-200 bg-white shadow-xs overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead onClick={() => handleSort('title')} className="cursor-pointer select-none">
                <div className="flex items-center gap-1">Title <ArrowUpDown className="h-3.5 w-3.5" /></div>
              </TableHead>
              <TableHead onClick={() => handleSort('priority')} className="cursor-pointer select-none w-28">
                <div className="flex items-center gap-1">Priority <ArrowUpDown className="h-3.5 w-3.5" /></div>
              </TableHead>
              <TableHead onClick={() => handleSort('isCompleted')} className="cursor-pointer select-none w-32">
                <div className="flex items-center gap-1">Status <ArrowUpDown className="h-3.5 w-3.5" /></div>
              </TableHead>
              <TableHead onClick={() => handleSort('createdAt')} className="cursor-pointer select-none w-44">
                <div className="flex items-center gap-1">Created <ArrowUpDown className="h-3.5 w-3.5" /></div>
              </TableHead>
              <TableHead className="w-28 text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredTasks.length === 0 ? (
              <TableRow><TableCell colSpan={5} className="h-24 text-center text-slate-500">No tasks found.</TableCell></TableRow>
            ) : (
              filteredTasks.map((t) => (
                <TableRow key={t.id}>
                  <TableCell className={`font-medium ${t.isCompleted ? 'line-through text-slate-400' : 'text-slate-900'}`}>{t.title}</TableCell>
                  <TableCell>{getPriorityBadge(t.priority)}</TableCell>
                  <TableCell>
                    {t.isCompleted ? <Badge variant="success">Done</Badge> : <Badge variant="outline">Pending</Badge>}
                  </TableCell>
                  <TableCell className="text-xs text-slate-500">{new Date(t.createdAt).toLocaleString()}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-1.5">
                      {canComplete && !t.isCompleted && (
                        <Button size="sm" variant="outline" className="h-8 px-2 text-emerald-600 hover:bg-emerald-50" onClick={() => completeTask(t.id)} disabled={isCompleting}>
                          <CheckCircle2 className="h-4 w-4" />
                        </Button>
                      )}
                      {canDelete && (
                        <Button size="sm" variant="outline" className="h-8 px-2 text-red-600 hover:bg-red-50" onClick={() => deleteTask(t.id)} disabled={isDeleting}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
