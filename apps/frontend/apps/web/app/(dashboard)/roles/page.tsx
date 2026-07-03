"use client";

import { useEffect, useState } from "react";
import { Button } from "@workspace/ui/components/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@workspace/ui/components/table";
import { Badge } from "@workspace/ui/components/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@workspace/ui/components/dialog";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";
import { Plus } from "lucide-react";
import { Skeleton } from "@workspace/ui/components/skeleton";
import { api } from "@/lib/api";
import type { RoleResponse, PermissionResponse } from "@/lib/types";

export default function RolesPage() {
  const [roles, setRoles] = useState<RoleResponse[]>([]);
  const [permissions, setPermissions] = useState<PermissionResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<RoleResponse | null>(null);
  const [roleName, setRoleName] = useState("");
  const [roleDescription, setRoleDescription] = useState("");
  const [selectedPermissions, setSelectedPermissions] = useState<number[]>([]);
  const [error, setError] = useState("");

  const fetchData = async () => {
    try {
      const [rolesData, permsData] = await Promise.all([
        api.get<RoleResponse[]>("/roles"),
        api.get<PermissionResponse[]>("/permissions"),
      ]);
      setRoles(rolesData);
      setPermissions(permsData);
    } catch (err) {
      console.error("Failed to fetch data:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleCreateRole = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    try {
      await api.post("/roles", { name: roleName, description: roleDescription });
      setIsCreateOpen(false);
      setRoleName(""); setRoleDescription("");
      fetchData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create role");
    }
  };

  const openEditPermissions = (role: RoleResponse) => {
    setEditingRole(role);
    setSelectedPermissions(role.permissions.map(p => p.id));
    setIsEditOpen(true);
  };

  const togglePermission = (permId: number) => {
    setSelectedPermissions(prev =>
      prev.includes(permId) ? prev.filter(id => id !== permId) : [...prev, permId]
    );
  };

  const handleSavePermissions = async () => {
    if (!editingRole) return;
    try {
      await api.post(`/roles/${editingRole.id}/permissions`, { permissionIds: selectedPermissions });
      setIsEditOpen(false);
      fetchData();
    } catch (err) {
      console.error("Failed to save permissions:", err);
    }
  };

  const handleDeleteRole = async (roleId: number) => {
    if (!confirm("Are you sure you want to delete this role?")) return;
    try {
      await api.delete(`/roles/${roleId}`);
      fetchData();
    } catch (err) {
      console.error("Failed to delete role:", err);
    }
  };

  // Group permissions by category
  const permissionsByCategory = permissions.reduce((acc, p) => {
    if (!acc[p.category]) acc[p.category] = [];
    acc[p.category].push(p);
    return acc;
  }, {} as Record<string, PermissionResponse[]>);

  if (isLoading) return <div className="space-y-3">{[1,2,3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Roles</h1>
        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
          <DialogTrigger asChild><Button><Plus className="mr-2 h-4 w-4" /> Create Role</Button></DialogTrigger>
          <DialogContent>
            <DialogHeader><DialogTitle>Create Role</DialogTitle></DialogHeader>
            <form onSubmit={handleCreateRole} className="space-y-4">
              {error && <div className="bg-destructive/10 text-destructive text-sm rounded-md p-3">{error}</div>}
              <div className="space-y-2"><Label>Name</Label><Input value={roleName} onChange={(e) => setRoleName(e.target.value)} required /></div>
              <div className="space-y-2"><Label>Description</Label><Input value={roleDescription} onChange={(e) => setRoleDescription(e.target.value)} /></div>
              <Button type="submit" className="w-full">Create Role</Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Description</TableHead>
            <TableHead>Permissions</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {roles.map((role) => (
            <TableRow key={role.id}>
              <TableCell className="font-medium">{role.name}</TableCell>
              <TableCell className="text-muted-foreground">{role.description || "—"}</TableCell>
              <TableCell><Badge variant="secondary">{role.permissions.length} permissions</Badge></TableCell>
              <TableCell className="flex gap-2">
                <Button variant="outline" size="sm" onClick={() => openEditPermissions(role)}>Edit Permissions</Button>
                <Button variant="ghost" size="sm" className="text-destructive" onClick={() => handleDeleteRole(role.id)}>Delete</Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent className="max-w-lg max-h-[80vh] overflow-y-auto">
          <DialogHeader><DialogTitle>Edit Permissions: {editingRole?.name}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            {Object.entries(permissionsByCategory).map(([category, perms]) => (
              <div key={category}>
                <h4 className="font-semibold text-sm mb-2">{category}</h4>
                <div className="grid grid-cols-1 gap-1">
                  {perms.map((perm) => (
                    <label key={perm.id} className="flex items-center gap-2 text-sm cursor-pointer hover:bg-accent rounded px-2 py-1">
                      <input type="checkbox" checked={selectedPermissions.includes(perm.id)} onChange={() => togglePermission(perm.id)} className="rounded" />
                      <span>{perm.name}</span>
                      <span className="text-xs text-muted-foreground ml-auto">{perm.description}</span>
                    </label>
                  ))}
                </div>
              </div>
            ))}
            <Button onClick={handleSavePermissions} className="w-full">Save Permissions</Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}