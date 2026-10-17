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
| New migration | — | `dotnet tool restore`, then `dotnet ef migrations add <Name>` |

## Standards

- Routes are `/api/v1/...`, and each one is defined by a `SPEC` in `../context/specs/` first.
- Controllers, DTOs, services and EF Core entities. Never return an entity from an endpoint.
- Validate every request.
- async/await for database, HTTP, PDF, SMS and AI calls.
- Every schema change is a migration, made with the pinned `dotnet-ef` in `.config/dotnet-tools.json`. Seed data every environment needs goes in a migration ([ADR-0004](../context/adr/ADR-0004-store-data-in-postgresql-through-ef-core-with-migrations.md)).
- Development applies migrations on start-up. Other environments run them as a deployment step.
- The connection string is `ConnectionStrings:DefaultConnection`, and the token key is `Jwt:SigningKey` (at least 32 bytes), both in user-secrets. The API refuses to start without the key.
- Read the time only through the injected `TimeProvider`, never `DateTime.Now` or `DateTime.UtcNow` ([ADR-0007](../context/adr/ADR-0007-business-dates-and-times-are-in-india-standard-time-from-an-injected-clock.md)).
- Errors are `ProblemDetails` with a `code`, made by `ApiProblems` ([ADR-0010](../context/adr/ADR-0010-the-app-shows-text-in-english-and-marathi-and-the-api-sends-codes.md)).
- Outside providers sit behind interfaces, such as `IOtpSender` ([ADR-0009](../context/adr/ADR-0009-outside-providers-sit-behind-interfaces-with-mocks-first.md)).
- Secrets live in user-secrets locally and in Azure Key Vault in production. Never in `appsettings*.json`.
- Comments are rare and name the artefact they implement: `// Max three OTP attempts: NFR-0002.`

## Tests

Test projects live in `tests/`, named `<Area>.Tests`, and are added to `OnlineDelivery.sln`.
They use xUnit and run the API in memory with `WebApplicationFactory<Program>` ([ADR-0003](../context/adr/ADR-0003-test-the-api-with-xunit-and-the-app-with-vitest.md)).
Every test class joins the `Api` collection and gets `Support/ApiFactory`, which starts one real
PostgreSQL in Docker with Testcontainers ([ADR-0004](../context/adr/ADR-0004-store-data-in-postgresql-through-ef-core-with-migrations.md)). Docker must be running for `dotnet test`.
A test that changes shared data puts it back in a `finally`. The factory replaces the clock with a
`FakeTimeProvider` and the OTP sender with `RecordingOtpSender`. Sign-in tests use `AuthSupport.NewMobileNumber()`,
so no two tests share a number.
`OnlineDelivery.Api.csproj` excludes `tests/**`, because the API project sits at the repository
root. An acceptance criterion binds to a test as
`test:online-delivery-api/tests/<Area>.Tests/<File>.cs::<Method>`.

## Frontend integration

- The React app is `../online-delivery-app`, at `http://localhost:5173` (the CORS origin allowed locally).
- This API runs at `http://localhost:5154`.
- Allowed browser origins are `Cors:AllowedOrigins` in each environment's appsettings ([ADR-0001](../context/adr/ADR-0001-browser-origins-allowed-by-the-api-come-from-configuration.md)). Outside Development, HTTP redirects to HTTPS ([ADR-0002](../context/adr/ADR-0002-redirect-http-to-https-outside-development-only.md)).

Every change here adds a line to `CHANGELOG.md` in the same commit.
