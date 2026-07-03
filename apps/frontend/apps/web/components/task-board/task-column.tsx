import type { TaskResponse } from "@/lib/types";
import { TaskCard } from "./task-card";

interface TaskColumnProps {
  title: string;
  tasks: TaskResponse[];
  count: number;
}

export function TaskColumn({ title, tasks, count }: TaskColumnProps) {
  return (
    <div className="flex flex-col gap-3 min-w-[280px] flex-1">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold text-sm">{title}</h3>
        <span className="text-xs text-muted-foreground bg-muted px-2 py-0.5 rounded-full">{count}</span>
      </div>
      <div className="flex flex-col gap-2">
        {tasks.length === 0 ? (
          <p className="text-xs text-muted-foreground text-center py-8">No tasks</p>
        ) : (
          tasks.map((task) => <TaskCard key={task.id} task={task} />)
        )}
      </div>
    </div>
  );
}