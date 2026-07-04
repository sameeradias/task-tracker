"use client";

import { useEffect, useRef } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { getTaskHubConnection, disconnectTaskHub } from "@/lib/signalr";
import type { TaskResponse } from "@/lib/types";

interface TaskEventCallbacks {
  onTaskCreated?: (task: TaskResponse) => void;
  onTaskUpdated?: (task: TaskResponse) => void;
  onTaskDeleted?: (data: { id: number }) => void;
}

export function useTaskEvents(callbacks: TaskEventCallbacks) {
  const callbacksRef = useRef(callbacks);
  callbacksRef.current = callbacks;

  useEffect(() => {
    const connection = getTaskHubConnection();

    connection.on("TaskCreated", (task: TaskResponse) => {
      callbacksRef.current.onTaskCreated?.(task);
    });

    connection.on("TaskUpdated", (task: TaskResponse) => {
      callbacksRef.current.onTaskUpdated?.(task);
    });

    connection.on("TaskDeleted", (data: { id: number }) => {
      callbacksRef.current.onTaskDeleted?.(data);
    });

    if (connection.state === HubConnectionState.Disconnected) {
      connection.start().catch((err) => {
        console.error("[SignalR] Failed to connect to TaskHub:", err);
      });
    }

    return () => {
      connection.off("TaskCreated");
      connection.off("TaskUpdated");
      connection.off("TaskDeleted");
      disconnectTaskHub();
    };
  }, []);
}