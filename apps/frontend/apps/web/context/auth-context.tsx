"use client";

import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from "react";
import { api } from "@/lib/api";
import { getToken, setToken, removeToken, decodeToken, isTokenExpired } from "@/lib/auth";
import type { AuthResponse, DecodedToken } from "@/lib/types";

interface AuthUser {
  userId: number;
  email: string;
  firstName: string;
  lastName?: string;
  role: string;
  permissions: string[];
}

interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  isAdmin: boolean;
  isSuperAdmin: boolean;
  hasPermission: (permission: string) => boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (firstName: string, lastName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const extractUser = useCallback((token: string): AuthUser | null => {
    const decoded = decodeToken(token);
    if (!decoded) return null;
    
    // Handle email claim which might be string or array
    const email = Array.isArray(decoded.email) ? decoded.email[0] : (decoded.email ?? "");
    const emailStr = typeof email === "string" ? email : String(email ?? "");
    
    return {
      userId: parseInt(decoded.user_id),
      email: emailStr,
      firstName: emailStr.includes("@") ? (emailStr.split("@")[0] || "User") : "User",
      role: decoded.user_role ?? "User",
      permissions: decoded.permissions ? (typeof decoded.permissions === "string" ? decoded.permissions.split(",").map(p => p.trim()) : []) : [],
    };
  }, []);

  useEffect(() => {
    const token = getToken();
    if (token && !isTokenExpired(token)) {
      const userData = extractUser(token);
      setUser(userData);
    }
    setIsLoading(false);
  }, [extractUser]);

  const login = async (email: string, password: string) => {
    const response = await api.post<AuthResponse>("/auth/login", { email, password });
    setToken(response.token);
    const userData = extractUser(response.token);
    if (userData) {
      userData.firstName = response.firstName;
      userData.lastName = response.lastName;
    }
    setUser(userData);
  };

  const register = async (firstName: string, lastName: string, email: string, password: string) => {
    const response = await api.post<AuthResponse>("/auth/register", {
      firstName,
      lastName,
      email,
      password,
    });
    setToken(response.token);
    const userData = extractUser(response.token);
    if (userData) {
      userData.firstName = firstName;
      userData.lastName = lastName;
    }
    setUser(userData);
  };

  const logout = () => {
    removeToken();
    setUser(null);
    window.location.href = "/login";
  };

  const isSuperAdmin = user?.role === "Super Admin";
  const isAdmin = isSuperAdmin || user?.role === "Admin";

  const hasPermission = (permission: string): boolean => {
    if (isSuperAdmin) return true;
    return user?.permissions.includes(permission) ?? false;
  };

  return (
    <AuthContext.Provider value={{ user, isLoading, isAdmin, isSuperAdmin, hasPermission, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}