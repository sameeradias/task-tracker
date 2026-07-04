# Task Management Module - Stage 4

## Overview

The Task Management Module provides complete CRUD functionality for managing tasks in the Task Tracker application. It implements a role-based ownership system where users can manage their own tasks, while administrators can manage all tasks. The module includes pagination, filtering, and proper security controls.

## What Was Built

The implementation includes a complete task management system with:

- **TaskItem Entity Model** - Core task entity with title, description, status, due date, and ownership
- **TaskItemStatus Enum** - Three status values: Todo (0), InProgress (1), Done (2)
- **Database Schema** - Full EF Core migration with proper indexes for performance
- **Service Layer** - TaskService with CRUD operations, pagination, and filtering
- **API Controller** - Five REST endpoints with role-based security
- **DTOs** - Complete request/response data models with validation
- **Ownership Security** - Users see only their tasks, admins manage all tasks

## Technical Implementation

### Files Created

#### Entity Model
- `apps/backend/Models/TaskItem.cs` - Task entity inheriting from BaseEntity
- `apps/backend/Models/Enums/TaskItemStatus.cs` - Status enumeration

#### Service Layer
- `apps/backend/Services/TaskService/ITaskService.cs` - Service interface
- `apps/backend/Services/TaskService/TaskService.cs` - Service implementation

#### API Layer  
- `apps/backend/Controllers/TaskController.cs` - REST API controller

#### Data Transfer Objects
- `apps/backend/DTOs/Task/CreateTaskRequestDTO.cs` - Create task request
- `apps/backend/DTOs/Task/UpdateTaskRequestDTO.cs` - Update task request  
- `apps/backend/DTOs/Task/TaskResponseDTO.cs` - Task response with owner info
- `apps/backend/DTOs/Task/TaskFilterRequestDTO.cs` - Query parameters
- `apps/backend/DTOs/Common/PaginatedResponseDTO.cs` - Generic pagination wrapper

#### Database
- `apps/backend/Migrations/20260703191147_AddTaskItem.cs` - EF Core migration
- `apps/backend/Migrations/20260703191147_AddTaskItem.Designer.cs` - Migration metadata

### Files Modified
- `apps/backend/Data/ApplicationDbContext.cs` - Added TaskItems DbSet and configuration
- `apps/backend/Program.cs` - Registered TaskService in dependency injection

### Key Components

#### TaskItem Entity
```csharp
public class TaskItem : BaseEntity
{
    public required string Title { get; set; }           // Max 200 chars
    public string? Description { get; set; }             // Max 2000 chars  
    public TaskItemStatus Status { get; set; }           // Todo/InProgress/Done
    public DateTime? DueDate { get; set; }               // Optional deadline
    public int OwnerId { get; set; }                     // FK to User
    public virtual User Owner { get; set; }              // Navigation property
}
```

#### Database Indexes
- `IX_TaskItems_OwnerId` - For filtering by owner
- `IX_TaskItems_Status` - For filtering by status  
- `IX_TaskItems_OwnerId_Status` - Composite index for combined filters

#### Ownership Security Model
- **Regular Users (User role)**: Can only see, create, edit, and delete their own tasks
- **Admin Users (Admin role)**: Can manage all tasks regardless of ownership
- **Owner Assignment**: Task owner is automatically set from JWT `user_id` claim on creation

## API Endpoints

### Base Route: `/api/tasks`

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | `/api/tasks` | READ:TASK, READ_OTHERS:TASK | Get paginated list of tasks |
| GET | `/api/tasks/{id}` | READ:TASK, READ_OTHERS:TASK | Get task by ID |
| POST | `/api/tasks` | CREATE:TASK | Create new task |
| PUT | `/api/tasks/{id}` | EDIT:TASK, EDIT_OTHERS:TASK | Update existing task |
| DELETE | `/api/tasks/{id}` | DELETE:TASK, DELETE_OTHERS:TASK | Soft delete task |

### Endpoint Details

#### GET /api/tasks - List Tasks
**Query Parameters:**
- `Status` (string, optional) - Filter by status: "Todo", "InProgress", "Done"
- `OwnerId` (int, optional) - Filter by owner (Admin only)
- `Page` (int, default: 1) - Page number (1-based)
- `PageSize` (int, default: 10) - Items per page

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Complete API documentation",
      "description": "Document all REST endpoints",
      "status": "InProgress",
      "dueDate": "2026-07-10T00:00:00Z",
      "ownerId": 1,
      "ownerName": "John Doe",
      "createdAt": "2026-07-03T19:18:00Z",
      "updatedAt": "2026-07-03T19:18:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

#### GET /api/tasks/{id} - Get Task by ID
**Response:** Single TaskResponseDTO object or 404 if not found.

#### POST /api/tasks - Create Task
**Request Body:**
```json
{
  "title": "New Task",
  "description": "Task description",
  "status": "Todo",
  "dueDate": "2026-07-15T00:00:00Z"
}
```
**Response:** 201 Created with TaskResponseDTO

#### PUT /api/tasks/{id} - Update Task
**Request Body:** Same as create, but all fields optional
**Response:** 200 OK with updated TaskResponseDTO

#### DELETE /api/tasks/{id} - Delete Task  
**Response:** 204 No Content (soft delete - sets IsDeleted = true)

## Usage Examples

### Authentication Required
All endpoints require a valid JWT token in the Authorization header:
```bash
Authorization: Bearer <jwt-token>
```

### Create a Task
```bash
curl -X POST http://localhost:5101/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "title": "Review pull request",
    "description": "Review PR #123 for bug fixes",
    "status": "Todo",
    "dueDate": "2026-07-05T17:00:00Z"
  }'
```

### List Tasks with Filtering
```bash
# Get in-progress tasks, page 1
curl "http://localhost:5101/api/tasks?status=InProgress&page=1&pageSize=5" \
  -H "Authorization: Bearer <token>"

# Admin: Get all tasks owned by user ID 2
curl "http://localhost:5101/api/tasks?ownerId=2" \
  -H "Authorization: Bearer <admin-token>"
```

### Update Task Status
```bash
curl -X PUT http://localhost:5101/api/tasks/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"status": "Done"}'
```

### Delete a Task
```bash
curl -X DELETE http://localhost:5101/api/tasks/1 \
  -H "Authorization: Bearer <token>"
```

## Configuration

No additional configuration is required. The module uses:

- **Database**: Configured PostgreSQL connection from `appsettings.json`
- **Authentication**: Existing JWT configuration  
- **Permissions**: Uses existing TASK permissions from DbInitializer seed data

## Security Features

### Permission Guards
Each endpoint is protected by PermissionGuard attributes that check for appropriate permissions:
- `READ:TASK` - Read own tasks
- `READ_OTHERS:TASK` - Read all tasks (Admin)
- `CREATE:TASK` - Create new tasks
- `EDIT:TASK` - Edit own tasks  
- `EDIT_OTHERS:TASK` - Edit all tasks (Admin)
- `DELETE:TASK` - Delete own tasks
- `DELETE_OTHERS:TASK` - Delete all tasks (Admin)

### Ownership Enforcement
- Regular users automatically have their `OwnerId` filter applied to all queries
- Task creation sets `OwnerId` from JWT `user_id` claim
- Update/delete operations verify ownership before proceeding
- Admin role bypasses all ownership checks

### Data Validation
- Title: Required, maximum 200 characters
- Description: Optional, maximum 2000 characters  
- Status: Must be valid enum value ("Todo", "InProgress", "Done")
- DueDate: Optional, valid DateTime format

## Database Migration

The module includes a complete EF Core migration (`20260703191147_AddTaskItem`) that:

- Creates the `TaskItems` table with all required columns
- Establishes foreign key relationship to Users table
- Creates performance indexes for common query patterns
- Inherits soft-delete behavior from BaseEntity

To apply the migration:
```bash
cd apps/backend
dotnet ef database update
```

## Performance Considerations

### Database Indexes
- **Single Indexes**: OwnerId, Status for individual filters
- **Composite Index**: (OwnerId, Status) for combined filtering
- **Default Ordering**: Tasks ordered by CreatedAt DESC (newest first)

### Query Optimization
- Uses `AsNoTracking()` by default for read operations
- Explicitly uses `AsTracking()` only for updates
- Includes Owner navigation property only when needed
- Implements proper pagination with Skip/Take

## Soft Delete Pattern

Tasks use soft delete via the `IsDeleted` flag inherited from BaseEntity:
- Deleted tasks are hidden from queries by global query filter
- Physical deletion is never performed, preserving audit trails
- `IsDeleted` is set to `true` and `UpdatedAt` is updated on deletion

## Status Workflow

Tasks follow a simple three-state workflow:
- **Todo (0)**: Initial state, task not started
- **InProgress (1)**: Task is actively being worked on  
- **Done (2)**: Task completed

Status transitions are not enforced - users can move between any states as needed.