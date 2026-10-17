# Changelog — online-delivery-api

Newest first. See [RULE-0016](../harness/rules/RULE-0016-changelog-format.md).

## 2026-10-18

### Added
- [US-0004](../context/stories/US-0004-anyone-can-list-the-active-cities-villages-and-categories.md): `GET /api/v1/cities` and `GET /api/v1/categories` ([SPEC-0002](../context/specs/SPEC-0002-cities-villages-and-categories.md)), with `ReferenceDataService` and DTOs
- PostgreSQL through EF Core and Npgsql ([ADR-0004](../context/adr/ADR-0004-store-data-in-postgresql-through-ef-core-with-migrations.md)): `AppDbContext`, the `City`, `Village` and `Category` entities, and the `AddReferenceData` migration with the seed cities, villages and categories
- `.config/dotnet-tools.json`, pinning `dotnet-ef`
- `Support/ApiFactory` in the tests: one PostgreSQL per test run, started by Testcontainers

### Changed
- Development applies pending migrations on start-up
- Every test class shares the `Api` collection, so all tests run against a real database
- `AGENTS.md` and `README.md`: migrations, the connection string name, and Docker for tests

## 2026-10-16

### Added
- [US-0001](../context/stories/US-0001-the-api-accepts-browser-calls-only-from-configured-origins.md): browser origins from `Cors:AllowedOrigins`, CORS before authorization, HTTPS redirect outside Development, and a `HealthResponse` DTO for [SPEC-0001](../context/specs/SPEC-0001-health-check-endpoint.md)
- `tests/OnlineDelivery.Api.Tests` (xUnit, [ADR-0003](../context/adr/ADR-0003-test-the-api-with-xunit-and-the-app-with-vitest.md)) and `OnlineDelivery.sln`, so format, build and test cover the tests
- `AGENTS.md`, with `CLAUDE.md` linked to it, routing to the workspace harness
- User secrets, holding the connection string; `.env.example` for Docker Compose

### Changed
- CI's test step passes when there is no `tests/` folder yet
- The PostgreSQL password comes from `.env` and user-secrets, never a tracked file ([RULE-0014](../harness/rules/RULE-0014-no-secrets-in-the-repository.md))
- `OnlineDelivery.Api.http` and `README.md` use port 5154 and the health endpoint

### Removed
- The Swagger template comment in `Program.cs`, which named no artefact ([RULE-0004](../harness/rules/RULE-0004-comments-are-short-and-name-the-artefact.md))
- The `WeatherForecast` template controller and model
