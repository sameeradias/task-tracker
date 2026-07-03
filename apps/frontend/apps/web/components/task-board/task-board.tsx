"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { TaskResponse, PaginatedResponse } from "@/lib/types";
import { TaskColumn } from "./task-column";
import { Skeleton } from "@workspace/ui/components/skeleton";

export function TaskBoard() {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchTasks() {
      try {
        const response = await api.get<PaginatedResponse<TaskResponse>>("/tasks?pageSize=100");
        setTasks(response.items);
      } catch (error) {
        console.error("Failed to fetch tasks:", error);
      } finally {
        setIsLoading(false);
      }
    }
    fetchTasks();
  }, []);

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {[1, 2, 3].map((i) => (
          <div key={i} className="space-y-3">
            <Skeleton className="h-6 w-24" />
            <Skeleton className="h-24 w-full" />
            <Skeleton className="h-24 w-full" />
          </div>
        ))}
      </div>
    );
  }

  const todoTasks = tasks.filter((t) => t.status === "Todo");
  const inProgressTasks = tasks.filter((t) => t.status === "InProgress");
  const doneTasks = tasks.filter((t) => t.status === "Done");

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      <TaskColumn title="Todo" tasks={todoTasks} count={todoTasks.length} />
      <TaskColumn title="In Progress" tasks={inProgressTasks} count={inProgressTasks.length} />
      <TaskColumn title="Done" tasks={doneTasks} count={doneTasks.length} />
    </div>
  );
}