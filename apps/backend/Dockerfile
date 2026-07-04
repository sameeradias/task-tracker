# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY backend.csproj ./
RUN dotnet restore

# Copy source code and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Configure environment
ENV ASPNETCORE_URLS=http://+:5101
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5101

ENTRYPOINT ["dotnet", "backend.dll"]