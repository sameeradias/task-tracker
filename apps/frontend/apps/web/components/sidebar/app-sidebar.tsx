"use client";

import { LayoutDashboard, CheckSquare, Users, Shield, Lock } from "lucide-react";
import { Sidebar, SidebarContent, SidebarFooter, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@workspace/ui/components/sidebar";
import { NavMain } from "@/components/sidebar/nav-main";
import { NavUser } from "@/components/sidebar/nav-user";
import { useAuth } from "@/context/auth-context";

export function AppSidebar() {
  const { user, hasPermission, isSuperAdmin } = useAuth();

  const mainNav = [
    { title: "Dashboard", url: "/", icon: LayoutDashboard },
    { title: "Tasks", url: "/tasks", icon: CheckSquare },
  ];

  const adminNav = [
    ...(isSuperAdmin || hasPermission("VIEW_MODULE:USER") || hasPermission("READ_OTHERS:USER")
      ? [{ title: "Users", url: "/users", icon: Users }] : []),
    ...(isSuperAdmin || hasPermission("VIEW_MODULE:ROLE") || hasPermission("READ:ROLE")
      ? [{ title: "Roles", url: "/roles", icon: Shield }] : []),
    ...(isSuperAdmin || hasPermission("VIEW_MODULE:PERMISSION") || hasPermission("READ:PERMISSION")
      ? [{ title: "Permissions", url: "/permissions", icon: Lock }] : []),
  ];

  return (
    <Sidebar>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <a href="/">
              <SidebarMenuButton size="lg">
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                  <CheckSquare className="size-4" />
                </div>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold">Task Tracker</span>
                  <span className="truncate text-xs text-muted-foreground">{user?.role || "User"}</span>
                </div>
              </SidebarMenuButton>
            </a>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={mainNav} label="Navigation" />
        {adminNav.length > 0 && <NavMain items={adminNav} label="Administration" />}
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
    </Sidebar>
  );
}