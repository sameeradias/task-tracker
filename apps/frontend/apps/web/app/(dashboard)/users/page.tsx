"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/auth-context";
import { Button } from "@workspace/ui/components/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@workspace/ui/components/table";
import { Badge } from "@workspace/ui/components/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@workspace/ui/components/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@workspace/ui/components/dialog";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";
import { Plus } from "lucide-react";
import { Skeleton } from "@workspace/ui/components/skeleton";
import { api } from "@/lib/api";
import type { UserResponse, RoleResponse } from "@/lib/types";

export default function UsersPage() {
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [roles, setRoles] = useState<RoleResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [selectedRoleId, setSelectedRoleId] = useState<string>("");
  const [error, setError] = useState("");
  const { user: currentUser } = useAuth();

  const fetchData = async () => {
    try {
      const [usersData, rolesData] = await Promise.all([
        api.get<UserResponse[]>("/users"),
        api.get<RoleResponse[]>("/roles"),
      ]);
      setUsers(usersData);
      setRoles(rolesData);
    } catch (err) {
      console.error("Failed to fetch data:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    try {
      await api.post("/users", {
        firstName, lastName, email, password,
        roleId: selectedRoleId ? parseInt(selectedRoleId) : undefined,
      });
      setIsCreateOpen(false);
      setFirstName(""); setLastName(""); setEmail(""); setPassword(""); setSelectedRoleId("");
      fetchData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create user");
    }
  };

  const handleRoleChange = async (userId: number, roleId: string) => {
    try {
      if (roleId) {
        await api.post(`/users/${userId}/role`, { roleId: parseInt(roleId) });
      } else {
        await api.delete(`/users/${userId}/role`);
      }
      fetchData();
    } catch (err) {
      console.error("Failed to assign role:", err);
    }
  };

  const handleDeleteUser = async (userId: number) => {
    if (!confirm("Are you sure you want to delete this user?")) return;
    try {
      await api.delete(`/users/${userId}`);
      fetchData();
    } catch (err) {
      console.error("Failed to delete user:", err);
    }
  };

  if (isLoading) return <div className="space-y-3">{[1,2,3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Users</h1>
        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
          <DialogTrigger>
            <Button><Plus className="mr-2 h-4 w-4" /> Create User</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader><DialogTitle>Create User</DialogTitle></DialogHeader>
            <form onSubmit={handleCreateUser} className="space-y-4">
              {error && <div className="bg-destructive/10 text-destructive text-sm rounded-md p-3">{error}</div>}
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2"><Label>First Name</Label><Input value={firstName} onChange={(e) => setFirstName(e.target.value)} required /></div>
                <div className="space-y-2"><Label>Last Name</Label><Input value={lastName} onChange={(e) => setLastName(e.target.value)} /></div>
              </div>
              <div className="space-y-2"><Label>Email</Label><Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></div>
              <div className="space-y-2"><Label>Password</Label><Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} /></div>
              <div className="space-y-2">
                <Label>Role</Label>
                <Select value={selectedRoleId} onValueChange={(value) => setSelectedRoleId(value || "")}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select role">
                      {selectedRoleId ? roles.find(r => String(r.id) === selectedRoleId)?.name || "Select role" : "Select role"}
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (<SelectItem key={role.id} value={String(role.id)}>{role.name}</SelectItem>))}
                  </SelectContent>
                </Select>
              </div>
              <Button type="submit" className="w-full">Create User</Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Role</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.map((user) => (
            <TableRow key={user.id}>
              <TableCell className="font-medium">{user.firstName} {user.lastName}</TableCell>
              <TableCell>{user.email}</TableCell>
              <TableCell>
                <Select value={roles.find(r => r.name === user.roleName)?.id?.toString() || ""} onValueChange={(v) => handleRoleChange(user.id, v || "")}>
                  <SelectTrigger className="w-[160px] h-8">
                    <SelectValue placeholder="No role">
                      {user.roleName || "No role"}
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (<SelectItem key={role.id} value={String(role.id)}>{role.name}</SelectItem>))}
                  </SelectContent>
                </Select>
              </TableCell>
              <TableCell><Badge variant={user.isActive ? "default" : "secondary"}>{user.isActive ? "Active" : "Inactive"}</Badge></TableCell>
              <TableCell>
                {currentUser?.userId !== user.id && (
                  <Button variant="ghost" size="sm" className="text-destructive" onClick={() => handleDeleteUser(user.id)}>Delete</Button>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}