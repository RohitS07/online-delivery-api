# AGENTS.md — Online Delivery API

The .NET REST API for the online delivery platform. The harness and the artefacts this code
implements live in the workspace above this repository: start at `../AGENTS.md`.

## Tech stack

ASP.NET Core Web API (.NET 8), PostgreSQL, Entity Framework Core, QuestPDF, an SMS OTP provider, Azure hosting.

## Commands

Run from the workspace root, or the `dotnet` command directly here.

| Task | Workspace | Here |
|---|---|---|
| Restore | `make install` | `dotnet restore` |
| Format | `make fmt-api` | `dotnet format OnlineDelivery.Api.csproj` |
| Lint | `make lint-api` | `dotnet format OnlineDelivery.Api.csproj --verify-no-changes` |
| Type check | `make typecheck-api` | `dotnet build -warnaserror` |
| Test | `make test-api` | `dotnet test tests/<Name>.Tests` |
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

Test projects live in `tests/`, named `<Area>.Tests`, with a `*Tests.csproj`. The API project
sits at the repository root, so add `<Compile Remove="tests/**" />` to `OnlineDelivery.Api.csproj`
together with the first test project, or the API compiles the tests too. An acceptance criterion
binds to a test as `test:tests/<Area>.Tests/<File>.cs::<Method>`.

## Frontend integration

- The React app is `../online-delivery-app`, at `http://localhost:5173` (the CORS origin allowed locally).
- This API runs at `http://localhost:5154`.

Every change here adds a line to `CHANGELOG.md` in the same commit.
