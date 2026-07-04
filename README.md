# Task Tracker

A full-stack task management application with role-based access control, real-time updates, and containerized deployment. Built with .NET 10, Next.js 16, PostgreSQL, and SignalR.

## Features

- **Authentication & Authorization** — JWT-based auth with granular permission system
- **Role-Based Access Control** — Super Admin, Admin, User roles with configurable permissions
- **Task Management** — Full CRUD with status tracking (Todo, InProgress, Done)
- **Real-Time Updates** — SignalR WebSocket for live task board synchronization
- **Task Board** — Kanban-style board with drag-free status columns
- **Pagination & Filtering** — Filter tasks by status and owner with paginated results
- **User Management** — Create users, assign roles, manage permissions
- **Dark Mode** — Full dark/light theme support
- **Containerized Deployment** — Docker + Docker Compose + GitHub Actions CI/CD

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core |
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS v4, shadcn/ui |
| Database | PostgreSQL 15 with EF Core migrations |
| Real-Time | SignalR (WebSocket) |
| Auth | JWT Bearer tokens, BCrypt password hashing |
| CI/CD | GitHub Actions, Docker, AWS ECR |
| UI Components | shadcn/ui (base-nova style) with Lucide icons |

## Architecture

```
task-tracker/
├── apps/
│   ├── backend/                 # .NET 10 Web API
│   │   ├── Controllers/         # Thin API controllers
│   │   ├── Services/            # Business logic (SOLID)
│   │   │   ├── AuthService/
│   │   │   ├── UserService/
│   │   │   ├── TaskService/
│   │   │   ├── RoleService/
│   │   │   └── PermissionService/
│   │   ├── Models/              # EF Core entities
│   │   ├── DTOs/                # Request/Response DTOs
│   │   ├── Hubs/                # SignalR hubs
│   │   ├── Data/                # DbContext & migrations
│   │   └── Attributes/          # PermissionGuard
│   └── frontend/                # Next.js 16 monorepo
│       ├── apps/web/            # Main web application
│       │   ├── app/(auth)/      # Login/Register pages
│       │   ├── app/(dashboard)/ # Protected pages
│       │   ├── components/      # UI components
│       │   ├── lib/             # API client, auth utils
│       │   └── context/         # React contexts
│       └── packages/ui/         # Shared shadcn components
├── .github/workflows/           # CI/CD pipeline
├── docker-compose.yml           # Production orchestration
├── deploy.sh                    # Server deployment script
└── specs/                       # Implementation plans
```

## Local Development Setup

### Prerequisites

- [Node.js 20+](https://nodejs.org/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- (Optional) [Docker](https://www.docker.com/) for containerized PostgreSQL

### 1. Clone the Repository

```bash
git clone <repository-url>
cd task-tracker
```

### 2. Start PostgreSQL

**Option A: Local PostgreSQL**
```bash
createdb tasktracker
```

**Option B: Docker**
```bash
docker run --name tasktracker-postgres \
  -e POSTGRES_PASSWORD=123456 \
  -e POSTGRES_DB=tasktracker \
  -p 5432:5432 -d postgres:15-alpine
```

### 3. Setup Backend

```bash
cd apps/backend

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the backend (port 5101)
dotnet run
```

The backend will:
- Start on `http://localhost:5101`
- Automatically seed permissions, roles, and a default Super Admin user
- Expose Swagger UI at `http://localhost:5101/swagger`

**Default Super Admin credentials:**
- Email: `admin@tasktracker.com`
- Password: `Admin@123`

### 4. Setup Frontend

```bash
# From project root
cd apps/frontend/apps/web

# Create environment file
cp .env.example .env
# .env should contain: BACKEND_API_URL=http://localhost:5101

# Go back to project root and install all dependencies
cd /path/to/task-tracker
npm install

# Run frontend (port 3000)
npm run dev:frontend
```

The frontend will start at `http://localhost:3000` and proxy API requests to the backend.

### 5. Verify Setup

```bash
# Health check
curl http://localhost:5101/health

# Open browser
open http://localhost:3000
# Login with admin@tasktracker.com / Admin@123
```

### Running Both Together

```bash
# From project root - runs backend + frontend concurrently
npm run dev
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/register | No | Register new user |
| POST | /api/auth/login | No | Login and get JWT |
| GET | /api/tasks | Yes | List tasks (paginated, filtered) |
| GET | /api/tasks/:id | Yes | Get task by ID |
| POST | /api/tasks | Yes | Create task |
| PUT | /api/tasks/:id | Yes | Update task |
| DELETE | /api/tasks/:id | Yes | Delete task (soft) |
| GET | /api/users | Yes | List users |
| POST | /api/users | Yes | Create user |
| PUT | /api/users/:id | Yes | Update user |
| DELETE | /api/users/:id | Yes | Delete user |
| POST | /api/users/:id/role | Yes | Assign role to user |
| GET | /api/roles | Yes | List roles |
| POST | /api/roles | Yes | Create role |
| POST | /api/roles/:id/permissions | Yes | Assign permissions |
| GET | /api/permissions | Yes | List all permissions |
| GET | /api/permissions/my | Yes | Get current user's permissions |
| GET | /health | No | Health check |

## Testing with Postman

The project includes a complete Postman collection for API testing.

### Import Collection

1. Open Postman
2. Click **Import** → select `TaskTracker.postman_collection.json` from project root
3. Also import `TaskTracker.postman_environment.json`
4. Select the **"Task Tracker - Local"** environment from the environment dropdown

### Run Tests

1. **Login first** — Run the `Auth > Login` request (pre-configured with admin credentials)
   - The JWT token is automatically saved to the collection variable `{{token}}`
   - All subsequent requests will use this token automatically

2. **Test any endpoint** — All requests are pre-configured with:
   - Correct URLs using `{{baseUrl}}`
   - Authorization headers using `{{token}}`
   - Example request bodies

### Suggested Test Flow

```
1. Auth > Login (as Super Admin)
2. Roles > Get All Roles
3. Users > Create User (with a role)
4. Tasks > Create Task
5. Tasks > Get Tasks (see pagination)
6. Tasks > Update Task (change status)
7. Tasks > Delete Task
```

## Automated Testing

The project includes a comprehensive automated test suite using xUnit with EF Core InMemory provider.

### Running Tests

```bash
# Run all tests
cd apps/backend.Tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~TaskServiceTests"

# Run only unit tests (exclude integration)
dotnet test --filter "FullyQualifiedName~Services"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

### Test Coverage

| Test Class | Tests | Coverage |
|-----------|-------|----------|
| TaskServiceTests | 14 | CRUD, pagination, filtering, soft delete, ownership |
| AuthServiceTests | 7 | Register, login, JWT generation, claims |
| UserServiceTests | 8 | CRUD, password hashing, role assignment, Super Admin exclusion |
| RoleServiceTests | 7 | CRUD, permission assignment, Super Admin protections |
| AuthApiTests (Integration) | 5 | Register/Login endpoints, error responses |
| TaskApiTests (Integration) | 4 | Auth enforcement, CRUD endpoints |
| **Total** | **44** | |

### CI Integration

Tests run automatically on every push and pull request via GitHub Actions. The pipeline will fail if any test fails, blocking the deployment.

## Deployment

### Docker Compose (Production)

The project includes Docker containerization for all services.

```bash
# Copy environment template
cp .env.example .env
# Edit .env with production values (passwords, JWT secret, etc.)

# Build and run all services
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f
```

Services:
- **PostgreSQL** — Port 5432 (internal network)
- **Backend** — Port 5101 (exposed)
- **Frontend** — Port 3000 (exposed)

### CI/CD Pipeline (GitHub Actions)

The project uses GitHub Actions for automated build and deployment.

**Pipeline Flow:**
```
Push to main → Lint & Test → Build Docker Images → Push to AWS ECR → SSH Deploy
```

**Required GitHub Secrets:**

| Secret | Description |
|--------|-------------|
| `AWS_ACCESS_KEY_ID` | AWS IAM access key |
| `AWS_SECRET_ACCESS_KEY` | AWS IAM secret key |
| `AWS_REGION` | AWS region (e.g., `ap-south-1`) |
| `AWS_ECR_REGISTRY` | ECR registry URL |
| `SERVER_HOST` | Deployment server IP |
| `SERVER_USER` | SSH username |
| `SERVER_SSH_KEY` | SSH private key |
| `DEPLOY_PATH` | Path on server (e.g., `/opt/task-tracker`) |

**Required GitHub Environment Variable:**

| Variable | Description |
|----------|-------------|
| `BACKEND_API_URL` | Backend URL for frontend build (e.g., `http://backend:5101`) |

**Server Prerequisites:**
- Docker & Docker Compose installed
- AWS CLI configured
- `deploy.sh`, `docker-compose.yml`, and `.env` placed at `DEPLOY_PATH`

### Manual Deployment

```bash
# On the server
cd /opt/task-tracker
bash deploy.sh <ECR_REGISTRY> <IMAGE_TAG> <AWS_REGION>
```

## Design Decisions

1. **SOLID Services** — Each module (Auth, User, Task, Role, Permission) has its own service with interface + implementation. Controllers are thin routing layers.

2. **Permission-Based Access** — Granular permissions using `{ACTION}:{MODULE}` format (e.g., `READ:TASK`, `EDIT_OTHERS:TASK`). Only Super Admin bypasses checks.

3. **Soft Deletes** — All entities use `IsDeleted` flag via BaseEntity. No data is permanently removed.

4. **NoTracking by Default** — EF Core uses `QueryTrackingBehavior.NoTracking` for read performance. `AsTracking()` is used explicitly for writes.

5. **Hidden Super Admin** — The Super Admin role and user are invisible in the UI. They exist only for initial system setup.

6. **SignalR Real-Time** — Task mutations broadcast events to all connected clients. The Task Board updates instantly without polling.

7. **Monorepo Structure** — Frontend uses Turborepo with shared UI components package (`@workspace/ui`).

8. **JWT in Cookie + localStorage** — Token stored in both for middleware (server-side) and client-side API calls.

## Assumptions

- PostgreSQL is available locally on port 5432 for development
- Single role per user (one-to-one via UserRole table)
- Task ownership is set at creation time and cannot be transferred
- Admin role permissions are configured by Super Admin after initial setup
- The frontend proxies all API calls through Next.js rewrites (no direct backend access from browser)

## Future Improvements

- [ ] Automated unit and integration tests
- [ ] Drag-and-drop on the Task Board (status change via drag)
- [ ] Task comments and activity log
- [ ] Email notifications for task assignments
- [ ] User profile page with avatar upload
- [ ] Task search (full-text)
- [ ] Export tasks to CSV/PDF
- [ ] Rate limiting on API endpoints
- [ ] Refresh token rotation for JWT
- [ ] Multi-tenant support

## License

ISC