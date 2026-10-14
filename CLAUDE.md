cd ~/projects/online-delivery-api

cat > CLAUDE.md <<'EOF'
# Online Delivery API

.NET REST API for the Online Delivery application.

## Tech Stack
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- QuestPDF
- SMS OTP provider
- Azure hosting

## Commands
- Restore: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run`
- Test: `dotnet test`
- Start PostgreSQL: `docker compose up -d`
- Stop PostgreSQL: `docker compose down`

## Standards
- Use `/api/v1/...` route naming.
- Use controllers, DTOs, services, and EF Core entities.
- Validate all incoming requests.
- Use async/await for database, HTTP, PDF, SMS, and AI operations.
- Do not expose database entities directly through API responses.
- Store secrets in user-secrets locally and Azure Key Vault in production.
- Use migrations for every database schema change.

## Frontend Integration
- React UI repository: `online-delivery-app`
- Allowed local development origin: `http://localhost:5173`
- API base URL: `http://localhost:5000`
EOF