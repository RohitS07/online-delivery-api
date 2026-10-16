# AGENTS.md — Online Delivery API

The .NET REST API for the online delivery platform. The harness and the artefacts this code
implements live in the workspace above this repository: start at `../AGENTS.md`.

## Tech stack

ASP.NET Core Web API (.NET 8), PostgreSQL, Entity Framework Core, QuestPDF, an SMS OTP provider, Azure hosting.

## Commands

Run from the workspace root, or the `dotnet` command directly here.

| Task | Workspace | Here |
|---|---|---|
| Restore | `make install` | `dotnet restore OnlineDelivery.sln` |
| Format | `make fmt-api` | `dotnet format OnlineDelivery.sln` |
| Lint | `make lint-api` | `dotnet format OnlineDelivery.sln --verify-no-changes` |
| Type check | `make typecheck-api` | `dotnet build OnlineDelivery.sln -warnaserror` |
| Test | `make test-api` | `dotnet test OnlineDelivery.sln` |
| Run on :5154 | `make run-api` | `dotnet run --launch-profile http` |
| PostgreSQL | `make db-up` / `make db-down` | `docker compose up -d` / `down` |

## Standards

- Routes are `/api/v1/...`, and each one is defined by a `SPEC` in `../context/specs/` first.
- Controllers, DTOs, services and EF Core entities. Never return an entity from an endpoint.
- Validate every request.
- async/await for database, HTTP, PDF, SMS and AI calls.
- Every schema change is a migration.
- Secrets live in user-secrets locally and in Azure Key Vault in production. Never in `appsettings*.json`.
- Comments are rare and name the artefact they implement: `// Max three OTP attempts: NFR-0002.`

## Tests

Test projects live in `tests/`, named `<Area>.Tests`, and are added to `OnlineDelivery.sln`.
They use xUnit and run the API in memory with `WebApplicationFactory<Program>` ([ADR-0003](../context/adr/ADR-0003-test-the-api-with-xunit-and-the-app-with-vitest.md)).
`OnlineDelivery.Api.csproj` excludes `tests/**`, because the API project sits at the repository
root. An acceptance criterion binds to a test as
`test:online-delivery-api/tests/<Area>.Tests/<File>.cs::<Method>`.

## Frontend integration

- The React app is `../online-delivery-app`, at `http://localhost:5173` (the CORS origin allowed locally).
- This API runs at `http://localhost:5154`.
- Allowed browser origins are `Cors:AllowedOrigins` in each environment's appsettings ([ADR-0001](../context/adr/ADR-0001-browser-origins-allowed-by-the-api-come-from-configuration.md)). Outside Development, HTTP redirects to HTTPS ([ADR-0002](../context/adr/ADR-0002-redirect-http-to-https-outside-development-only.md)).

Every change here adds a line to `CHANGELOG.md` in the same commit.
