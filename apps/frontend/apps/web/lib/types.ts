// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName?: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  email: string;
  firstName: string;
  lastName?: string;
  role?: string;
  expiresAt: string;
}

// Task types
export interface TaskResponse {
  id: number;
  title: string;
  description?: string;
  status: string;
  dueDate?: string;
  ownerId: number;
  ownerName: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status?: string;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: string;
  dueDate?: string;
}

export interface TaskFilter {
  status?: string;
  ownerId?: number;
  page?: number;
  pageSize?: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// User types
export interface UserResponse {
  id: number;
  firstName: string;
  lastName?: string;
  email: string;
  isActive: boolean;
  roleName?: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName?: string;
  email: string;
  password: string;
  roleId?: number;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
}

// Role types
export interface RoleResponse {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  permissions: PermissionResponse[];
}

export interface PermissionResponse {
  id: number;
  name: string;
  description?: string;
  category: string;
}

// JWT decoded token
export interface DecodedToken {
  user_id: string;
  email: string;
  user_role: string;
  permissions: string;
  exp: number;
}