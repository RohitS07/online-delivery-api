# Online Delivery API

.NET REST API for the Online Delivery application.

This API provides backend services for users, restaurants, menus, orders,
pricing, notifications, OTP-based authentication, PDF generation, and AI features.

## Tech Stack
- ASP.NET Core Web API (.NET 8)
- PostgreSQL
- Entity Framework Core
- QuestPDF for server-side PDF generation
- SMS provider for Indian mobile OTP
- Azure hosting

## Local Development

This repository sits inside the `online-delivery` workspace, which holds the harness and the
specs. From the workspace root, `make install`, `make db-up` and `make run-api` do the steps below.

1. Configure PostgreSQL for Docker Compose:
   ```bash
   cp .env.example .env    # then set POSTGRES_PASSWORD
   ```
2. Store the connection string, and any other secret, in user-secrets. Never in `appsettings*.json`:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Host=localhost;Port=5432;Database=online_delivery;Username=postgres;Password=<your password>"
   ```
3. Start the database and the API:
   ```bash
   docker compose up -d
   dotnet run --launch-profile http
   ```

In production, secrets come from Azure Key Vault.

## API Base URL
```text
http://localhost:5154/api/v1
```

Health check: `GET /api/v1/health`. Reference data: `GET /api/v1/cities`, `GET /api/v1/categories`.

## Database

In Development the API applies pending EF Core migrations when it starts. To add one:

```bash
dotnet tool restore
dotnet ef migrations add <Name>
```

Tests start their own PostgreSQL in Docker, so Docker must be running for `dotnet test`.
