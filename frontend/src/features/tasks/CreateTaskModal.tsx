import React, { useState } from 'react';
import { useCreateTaskMutation } from '@/features/tasks/tasksApi';
import { Priority } from '@/features/tasks/taskTypes';
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Plus } from 'lucide-react';

interface CreateTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateTaskModal({ isOpen, onClose }: CreateTaskModalProps) {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState<Priority>(Priority.Medium);
  const [createTask, { isLoading }] = useCreateTaskMutation();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;

    try {
      await createTask({
        title: title.trim(),
        priority,
        isCompleted: false,
      }).unwrap();
      setTitle('');
      setPriority(Priority.Medium);
      onClose();
    } catch (err) {
      console.error('Failed to create task:', err);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Create New Task"
      description="Add a task with title and priority level."
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-1.5">
          <label className="text-sm font-medium text-slate-700">Task Title</label>
          <Input
            type="text"
            placeholder="e.g. Verify database migration"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={200}
          />
        </div>

        <div className="space-y-1.5">
          <label className="text-sm font-medium text-slate-700">Priority</label>
          <select
            className="flex h-9 w-full rounded-md border border-slate-300 bg-white px-3 py-1 text-sm shadow-xs focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
            value={priority}
            onChange={(e) => setPriority(Number(e.target.value) as Priority)}
          >
            <option value={Priority.Low}>Low</option>
            <option value={Priority.Medium}>Medium</option>
            <option value={Priority.High}>High</option>
          </select>
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
          <Button type="button" variant="outline" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading} className="gap-1.5">
            <Plus className="h-4 w-4" />
            <span>{isLoading ? 'Creating...' : 'Create Task'}</span>
          </Button>
        </div>
      </form>
    </Modal>
  );
}
