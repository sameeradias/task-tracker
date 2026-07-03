"use client";

import { useEffect, useState } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@workspace/ui/components/table";
import { Badge } from "@workspace/ui/components/badge";
import { Input } from "@workspace/ui/components/input";
import { Skeleton } from "@workspace/ui/components/skeleton";
import { Search } from "lucide-react";
import { api } from "@/lib/api";
import type { PermissionResponse } from "@/lib/types";

export default function PermissionsPage() {
  const [permissions, setPermissions] = useState<PermissionResponse[]>([]);
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchPermissions() {
      try {
        const data = await api.get<PermissionResponse[]>("/permissions");
        setPermissions(data);
      } catch (err) {
        console.error("Failed to fetch permissions:", err);
      } finally {
        setIsLoading(false);
      }
    }
    fetchPermissions();
  }, []);

  const filtered = permissions.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    p.category.toLowerCase().includes(search.toLowerCase()) ||
    (p.description || "").toLowerCase().includes(search.toLowerCase())
  );

  const grouped = filtered.reduce((acc, p) => {
    if (!acc[p.category]) acc[p.category] = [];
    acc[p.category].push(p);
    return acc;
  }, {} as Record<string, PermissionResponse[]>);

  if (isLoading) return <div className="space-y-3">{[1,2,3,4].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Permissions</h1>
        <div className="relative w-64">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Search permissions..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
      </div>
      {Object.entries(grouped).map(([category, perms]) => (
        <div key={category}>
          <h3 className="font-semibold text-sm text-muted-foreground mb-2 uppercase tracking-wider">{category} Module</h3>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Permission</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Category</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {perms.map((perm) => (
                <TableRow key={perm.id}>
                  <TableCell className="font-mono text-sm">{perm.name}</TableCell>
                  <TableCell className="text-muted-foreground">{perm.description || "—"}</TableCell>
                  <TableCell><Badge variant="outline">{perm.category}</Badge></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      ))}
      <p className="text-sm text-muted-foreground">{permissions.length} permissions total across {Object.keys(grouped).length} modules</p>
    </div>
  );
}