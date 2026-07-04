"use client";

import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@workspace/ui/components/card";
import { Badge } from "@workspace/ui/components/badge";
import type { TaskResponse } from "@/lib/types";

const statusVariants: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
  Todo: "outline",
  InProgress: "secondary",
  Done: "default",
};

export function TaskCard({ task }: { task: TaskResponse }) {
  const dueDate = task.dueDate ? new Date(task.dueDate).toLocaleDateString() : null;

  return (
    <Link href={`/tasks/${task.id}`}>
      <Card className="hover:bg-accent/50 transition-colors cursor-pointer">
        <CardHeader className="p-3 pb-1">
          <CardTitle className="text-sm font-medium leading-tight">{task.title}</CardTitle>
        </CardHeader>
        <CardContent className="p-3 pt-1">
          {task.description && (
            <p className="text-xs text-muted-foreground line-clamp-2 mb-2">{task.description}</p>
          )}
          <div className="flex items-center justify-between gap-2">
            <Badge variant={statusVariants[task.status] || "outline"} className="text-xs">
              {task.status}
            </Badge>
            {dueDate && <span className="text-xs text-muted-foreground">{dueDate}</span>}
          </div>
          <p className="text-xs text-muted-foreground mt-1">{task.ownerName}</p>
        </CardContent>
      </Card>
    </Link>
  );
}