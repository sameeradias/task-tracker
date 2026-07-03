"use client";

import Link from "next/link";
import { Button } from "@workspace/ui/components/button";
import { Plus } from "lucide-react";
import { TaskBoard } from "@/components/task-board/task-board";

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Task Board</h1>
          <p className="text-muted-foreground text-sm">View and manage your tasks</p>
        </div>
        <Button asChild>
          <Link href="/dashboard/tasks/new">
            <Plus className="mr-2 h-4 w-4" /> New Task
          </Link>
        </Button>
      </div>
      <TaskBoard />
    </div>
  );
}