"use client";

import { LayoutDashboard, CheckSquare, Users, Shield, Lock } from "lucide-react";
import { Sidebar, SidebarContent, SidebarFooter, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@workspace/ui/components/sidebar";
import { NavMain } from "@/components/sidebar/nav-main";
import { NavUser } from "@/components/sidebar/nav-user";
import { useAuth } from "@/context/auth-context";

export function AppSidebar() {
  const { user, isAdmin } = useAuth();

  const mainNav = [
    { title: "Dashboard", url: "/dashboard", icon: LayoutDashboard },
    { title: "Tasks", url: "/dashboard/tasks", icon: CheckSquare },
  ];

  const adminNav = [
    { title: "Users", url: "/dashboard/users", icon: Users },
    { title: "Roles", url: "/dashboard/roles", icon: Shield },
    { title: "Permissions", url: "/dashboard/permissions", icon: Lock },
  ];

  return (
    <Sidebar>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <a href="/dashboard">
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                  <CheckSquare className="size-4" />
                </div>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold">Task Tracker</span>
                  <span className="truncate text-xs text-muted-foreground">{user?.role || "User"}</span>
                </div>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={mainNav} label="Navigation" />
        {isAdmin && <NavMain items={adminNav} label="Administration" />}
      </SidebarContent>
      <SidebarFooter>
        <NavUser />
      </SidebarFooter>
    </Sidebar>
  );
}