import { HubConnectionBuilder, HubConnection, LogLevel, HubConnectionState } from "@microsoft/signalr";
import { getToken } from "./auth";

let connection: HubConnection | null = null;

export function getTaskHubConnection(): HubConnection {
  if (connection) return connection;

  const token = getToken();

  connection = new HubConnectionBuilder()
    .withUrl("/hubs/tasks", {
      accessTokenFactory: () => token || "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

  return connection;
}

export function getConnectionState(): HubConnectionState {
  return connection?.state ?? HubConnectionState.Disconnected;
}

export function disconnectTaskHub(): void {
  if (connection) {
    connection.stop();
    connection = null;
  }
}