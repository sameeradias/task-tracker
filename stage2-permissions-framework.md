# Stage 2: Permissions Framework Documentation

## Overview

The Task Tracker Stage 2 Permissions Framework provides a comprehensive Role-Based Access Control (RBAC) system that secures application endpoints and manages user permissions through a flexible permission-role-user hierarchy. The framework implements granular permissions across four functional modules (TASK, USER, ROLE, PERMISSION) with two predefined roles (User and Admin).

## What Was Built

The implementation consists of:

- **Entity Models**: Complete RBAC data model with audit trails
- **Authorization Services**: Business logic for permission and role management
- **API Controllers**: Secured endpoints for permission and role operations
- **Authorization Attribute**: Declarative permission-based endpoint protection
- **Database Seeding**: Automated initialization of 24 permissions, roles, and admin user
- **DTOs**: Structured API contracts for request/response operations

## Technical Implementation

### Files Created/Modified

#### Models (`apps/backend/Models/`)
- `BaseEntity.cs` - Abstract base class with audit fields and soft delete
- `User.cs` - User entity with profile information and role relationships
- `Permission.cs` - Permission entity with ACTION:MODULE format
- `Role.cs` - Role entity for grouping permissions
- `RolePermission.cs` - Many-to-many relationship between roles and permissions
- `UserRole.cs` - Many-to-many relationship between users and roles

#### Services (`apps/backend/Services/Authorization/`)
- `IPermissionService.cs` - Permission service interface
- `PermissionService.cs` - Permission retrieval and user permission resolution
- `IRoleService.cs` - Role service interface  
- `RoleService.cs` - Complete role CRUD and role-permission management

#### Controllers (`apps/backend/Controllers/`)
- `PermissionController.cs` - Permission listing and category filtering endpoints
- `RoleController.cs` - Full role management API with permission assignment

#### DTOs (`apps/backend/DTOs/`)
- `PermissionResponseDTO.cs` - Permission data transfer object
- `RoleResponseDTO.cs` - Role data with embedded permissions
- `CreateRoleRequestDTO.cs` - Role creation request
- `UpdateRoleRequestDTO.cs` - Role update request
- `AssignPermissionsRequestDTO.cs` - Permission assignment to roles
- `AssignRoleRequestDTO.cs` - Role assignment to users

#### Authorization Infrastructure
- `PermissionGuardAttribute.cs` - Action filter for endpoint authorization
- `DbInitializer.cs` - Database seeding for permissions, roles, and admin user
- `ApplicationDbContext.cs` - Updated with permission entities and relationships

#### Configuration
- `Program.cs` - Service registration and automatic database seeding

## Permission Matrix

The framework defines 24 granular permissions across 4 functional modules:

### TASK Module (8 permissions)
| Permission | Description |
|------------|-------------|
| `READ:TASK` | Read tasks created by current user |
| `READ_OTHERS:TASK` | Read tasks created by other users |
| `CREATE:TASK` | Create new tasks |
| `EDIT:TASK` | Edit tasks created by current user |
| `EDIT_OTHERS:TASK` | Edit tasks created by other users |
| `DELETE:TASK` | Delete tasks created by current user |
| `DELETE_OTHERS:TASK` | Delete tasks created by other users |
| `VIEW_MODULE:TASK` | Access task management module |

### USER Module (8 permissions)
| Permission | Description |
|------------|-------------|
| `READ:USER` | Read own user profile |
| `READ_OTHERS:USER` | Read other users' profiles |
| `CREATE:USER` | Create new users |
| `EDIT:USER` | Edit own user profile |
| `EDIT_OTHERS:USER` | Edit other users' profiles |
| `DELETE:USER` | Delete own user account |
| `DELETE_OTHERS:USER` | Delete other users' accounts |
| `VIEW_MODULE:USER` | Access user management module |

### ROLE Module (6 permissions)
| Permission | Description |
|------------|-------------|
| `READ:ROLE` | Read role information |
| `CREATE:ROLE` | Create new roles |
| `EDIT:ROLE` | Edit existing roles |
| `DELETE:ROLE` | Delete roles |
| `ASSIGN:ROLE` | Assign roles to users |
| `VIEW_MODULE:ROLE` | Access role management module |

### PERMISSION Module (2 permissions)
| Permission | Description |
|------------|-------------|
| `READ:PERMISSION` | Read permission information |
| `VIEW_MODULE:PERMISSION` | Access permission management module |

## Role Definitions and Permission Assignments

### User Role
**Description**: Standard user who can manage their own tasks

**Permissions**:
- `READ:TASK` - Read own tasks
- `CREATE:TASK` - Create new tasks  
- `EDIT:TASK` - Edit own tasks
- `DELETE:TASK` - Delete own tasks
- `VIEW_MODULE:TASK` - Access task module
- `READ:USER` - Read own profile
- `EDIT:USER` - Edit own profile

### Admin Role
**Description**: Administrator who can manage all tasks and users

**Permissions**: All 24 permissions (complete system access)

**Special Behavior**: Admin role bypasses all permission checks in `PermissionGuardAttribute`

## API Endpoints Reference

### Permission Management

#### Get All Permissions
```http
GET /api/permissions
```
**Description**: Retrieve all permissions in the system  
**Authorization**: None (public endpoint)  
**Response**: Array of `PermissionResponseDTO`

#### Get Permissions by Category
```http
GET /api/permissions/category/{category}
```
**Description**: Filter permissions by category (TASK, USER, ROLE, PERMISSION)  
**Authorization**: Requires `READ:PERMISSION`  
**Response**: Array of `PermissionResponseDTO`

### Role Management

#### Get All Roles
```http
GET /api/roles
```
**Description**: Retrieve all roles with their permissions  
**Authorization**: Requires `READ:ROLE`  
**Response**: Array of `RoleResponseDTO`

#### Get Role by ID
```http
GET /api/roles/{id}
```
**Description**: Get specific role with permissions  
**Authorization**: Requires `READ:ROLE`  
**Response**: `RoleResponseDTO`

#### Create Role
```http
POST /api/roles
Content-Type: application/json

{
  "name": "string",
  "description": "string"
}
```
**Authorization**: Requires `CREATE:ROLE`  
**Response**: `RoleResponseDTO`

#### Update Role
```http
PUT /api/roles/{id}
Content-Type: application/json

{
  "name": "string", 
  "description": "string"
}
```
**Authorization**: Requires `EDIT:ROLE`  
**Response**: `RoleResponseDTO`

#### Delete Role
```http
DELETE /api/roles/{id}
```
**Authorization**: Requires `DELETE:ROLE`  
**Response**: 204 No Content

#### Assign Permissions to Role
```http
POST /api/roles/{id}/permissions
Content-Type: application/json

{
  "permissionIds": [1, 2, 3]
}
```
**Authorization**: Requires `EDIT:ROLE`  
**Response**: 200 OK

#### Get Role Permissions
```http
GET /api/roles/{id}/permissions
```
**Authorization**: Requires `READ:ROLE`  
**Response**: Array of `PermissionResponseDTO`

#### Assign Role to User
```http
POST /api/roles/assign
Content-Type: application/json

{
  "userId": 1,
  "roleId": 2
}
```
**Authorization**: Requires `ASSIGN:ROLE`  
**Response**: 200 OK

#### Remove Role from User
```http
DELETE /api/roles/{roleId}/users/{userId}
```
**Authorization**: Requires `ASSIGN:ROLE`  
**Response**: 204 No Content

## How to Use PermissionGuardAttribute

The `PermissionGuardAttribute` provides declarative permission-based authorization for controllers and actions.

### Basic Usage

```csharp
[PermissionGuard("READ:TASK")]
public async Task<IActionResult> GetTasks()
{
    // Only users with READ:TASK permission can access this endpoint
}
```

### Multiple Permissions (OR Logic)

```csharp
[PermissionGuard("READ:TASK", "READ_OTHERS:TASK")]
public async Task<IActionResult> GetAllTasks()
{
    // Users need either READ:TASK OR READ_OTHERS:TASK permission
}
```

### Controller-Level Authorization

```csharp
[PermissionGuard("VIEW_MODULE:TASK")]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    // All actions require VIEW_MODULE:TASK permission
    // Individual actions can add additional permission requirements
    
    [PermissionGuard("CREATE:TASK")]
    public async Task<IActionResult> CreateTask()
    {
        // Requires both VIEW_MODULE:TASK (controller) AND CREATE:TASK (action)
    }
}
```

### Authorization Behavior

1. **Authentication Check**: Verifies user is authenticated
2. **Admin Bypass**: Admin role bypasses all permission checks
3. **Permission Validation**: Checks if user has at least one required permission
4. **Claims Structure**: 
   - User role: `user_role` claim
   - Permissions: `permissions` claim (comma-separated string)

### Error Responses

**401 Unauthorized** (Not authenticated):
```json
{
  "message": "Authentication required."
}
```

**403 Forbidden** (Missing permissions):
```json
{
  "message": "You don't have permission to perform this action. Please contact your administrator.",
  "requiredPermissions": ["READ:TASK", "READ_OTHERS:TASK"]
}
```

## Database Seeding Behavior

The `DbInitializer` automatically seeds the database on application startup with a complete permission structure.

### Seeding Process

1. **Permissions**: Creates all 24 permissions across 4 modules
2. **Roles**: Creates User and Admin roles
3. **Role-Permission Assignment**: 
   - User role: 7 basic permissions
   - Admin role: All 24 permissions
4. **Default Admin User**: Creates `admin@tasktracker.com` with Admin role

### Key Features

- **Idempotent**: Safe to run multiple times without duplicates
- **Automatic**: Runs on every application startup
- **Audited**: All entities include creation timestamps
- **Trackable**: Uses EF tracking for seeding operations
- **Logged**: Console output for monitoring seeding progress

### Seeded Data Summary

- **24 Permissions** across TASK, USER, ROLE, PERMISSION modules
- **2 Roles** (User with 7 permissions, Admin with all permissions)
- **1 Admin User** (`admin@tasktracker.com`) with placeholder password
- **32 Role-Permission assignments** (7 for User role, 25 for Admin role)

### Database Schema

The framework uses PostgreSQL with the following key tables:

- `Users` - User accounts and profiles
- `Roles` - Role definitions
- `Permissions` - Individual permission definitions
- `UserRoles` - User-role assignments (many-to-many)
- `RolePermissions` - Role-permission assignments (many-to-many)

All tables include audit fields (CreatedAt, UpdatedAt, CreatedBy, LastUpdatedBy) and soft delete support (IsDeleted flag).

## Configuration

### Service Registration

The framework registers authorization services in `Program.cs`:

```csharp
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
```

### Database Configuration

Uses Entity Framework Core with PostgreSQL:
- NoTracking by default for performance
- Soft delete query filter on BaseEntity
- Unique constraints on permission names, role names, and user emails
- Cascade deletes for junction tables

### Health Checks

Database connectivity health check available at `/health` endpoint.

---

**Implementation Status**: ✅ Complete  
**Next Stage**: Task entity model and CRUD operations with permission integration