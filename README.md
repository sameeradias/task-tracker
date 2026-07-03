# Task Tracker

A modern full-stack task management application built with .NET 10 and Next.js.

## 🚀 Quick Start

### Prerequisites
- Node.js 20+
- .NET 10 SDK
- PostgreSQL

### Setup

1. **Clone and setup:**
   ```bash
   git clone <repository-url>
   cd task-tracker
   ```

2. **Start PostgreSQL:**
   ```bash
   # Local PostgreSQL
   createdb tasktracker
   
   # Or Docker
   docker run --name tasktracker-postgres \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=tasktracker \
     -p 5432:5432 -d postgres:15
   ```

3. **Setup backend:**
   ```bash
   dotnet tool restore
   cd apps/backend
   dotnet restore
   dotnet ef database update
   dotnet watch run
   ```
   Backend runs at `https://localhost:5101`

4. **Setup frontend:**
   ```bash
   cd apps/frontend
   npm install
   
   # Create .env file
   echo "BACKEND_API_URL=http://localhost:5101" > apps/web/.env
   
   npm run dev
   ```
   Frontend runs at `http://localhost:3000`

5. **Verify setup:**
   ```bash
   curl https://localhost:5101/health
   # Should return: Healthy
   ```

## 📋 Implementation Status

### ✅ Stage 1: Foundation (Complete)
- EF Core + PostgreSQL backend setup
- Database connectivity and health checks
- Next.js frontend with API proxy configuration
- Environment configuration
- Initial EF Core migration infrastructure

### 🔄 Planned Stages
- **Stage 2**: User authentication and authorization (JWT, RBAC)
- **Stage 3**: Task entity model and CRUD operations
- **Stage 4**: Frontend task management UI with filtering/pagination
- **Stage 5**: Real-time updates with SignalR
- **Stage 6**: Testing infrastructure and CI/CD pipeline
- **Stage 7**: Containerization and deployment

## 🏗 Architecture

### Backend (.NET 10)
- **Framework**: ASP.NET Core Web API
- **Database**: PostgreSQL with EF Core
- **Architecture**: Clean architecture principles
- **Health Checks**: Database connectivity monitoring
- **Configuration**: Environment-based settings

### Frontend (Next.js)
- **Framework**: Next.js 16 with TypeScript
- **Monorepo**: Turborepo for workspace management
- **API Communication**: Proxy configuration for development
- **Styling**: Tailwind CSS (configured)

### Database
- **Provider**: PostgreSQL 15+
- **ORM**: Entity Framework Core 10
- **Migrations**: Code-first with EF Core migrations
- **Performance**: NoTracking queries by default

## 📚 Documentation

- **[Setup Guide](./SETUP.md)** - Comprehensive setup instructions
- **[Architecture Decisions](./specs/)** - Design decisions and specifications

## 🛠 Development

### Available Commands

```bash
# Run both backend and frontend
npm run dev

# Backend only
npm run dev:backend

# Frontend only
npm run dev:frontend

# Build all
npm run build

# Backend build
npm run build:backend

# Frontend build
npm run build:frontend
```

### Project Structure

```
task-tracker/
├── apps/
│   ├── backend/           # .NET 10 Web API
│   │   ├── Program.cs     # Application entry point
│   │   ├── Data/          # EF Core DbContext and migrations
│   │   └── appsettings.*  # Configuration files
│   └── frontend/          # Next.js monorepo
│       └── apps/web/      # Main web application
├── specs/                 # Implementation specifications
├── SETUP.md              # Detailed setup guide
└── package.json          # Root workspace configuration
```

## 🔍 Health Monitoring

The backend provides health check endpoints for monitoring:

- **Health Check**: `GET /health` - Database connectivity status
- **Response**: JSON with health status and database connection details

## 🤝 Contributing

1. Read the implementation plan in `specs/`
2. Follow the existing code patterns and architecture
3. Ensure all tests pass before submitting
4. Update documentation for new features

## 📄 License

ISC License
