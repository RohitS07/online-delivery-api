# Changelog — online-delivery-api

Newest first. See [RULE-0016](../harness/rules/RULE-0016-changelog-format.md).

## 2026-10-18

### Added
- [US-0005](../context/stories/US-0005-a-person-proves-their-mobile-number-and-gets-a-token.md): `POST /api/v1/auth/otp`, `POST /api/v1/auth/otp/verify` and `GET /api/v1/auth/me` ([SPEC-0003](../context/specs/SPEC-0003-sign-in-with-a-mobile-otp.md)), with codes hashed and limited (the attempt, single-use and request limits hold under parallel requests), HS256-only 7-day access tokens and 15-minute registration tokens ([ADR-0006](../context/adr/ADR-0006-sign-in-with-a-mobile-otp-and-a-jwt-bearer-token.md))
- `User` and `OtpChallenge` entities, and the `AddUsersAndOtp` migration
- JWT bearer authentication, with lifetimes checked against the injected `TimeProvider` ([ADR-0007](../context/adr/ADR-0007-business-dates-and-times-are-in-india-standard-time-from-an-injected-clock.md)), and a check on each authorized request that the user still exists, has the token's role and is not blocked
- `IOtpSender` with `MockOtpSender`, which logs codes in Development only ([ADR-0009](../context/adr/ADR-0009-outside-providers-sit-behind-interfaces-with-mocks-first.md))
- The first admin from `Auth:FirstAdmin`, and demo users in Development
- `ApiProblems`: errors as `ProblemDetails` with a `code` ([ADR-0010](../context/adr/ADR-0010-the-app-shows-text-in-english-and-marathi-and-the-api-sends-codes.md))
- Tests: `FakeTimeProvider`, `RecordingOtpSender` and `AuthSupport`
- [US-0004](../context/stories/US-0004-anyone-can-list-the-active-cities-villages-and-categories.md): `GET /api/v1/cities` and `GET /api/v1/categories` ([SPEC-0002](../context/specs/SPEC-0002-cities-villages-and-categories.md)), with `ReferenceDataService` and DTOs
- PostgreSQL through EF Core and Npgsql ([ADR-0004](../context/adr/ADR-0004-store-data-in-postgresql-through-ef-core-with-migrations.md)): `AppDbContext`, the `City`, `Village` and `Category` entities, and the `AddReferenceData` migration with the seed cities, villages and categories
- `.config/dotnet-tools.json`, pinning `dotnet-ef`
- `Support/ApiFactory` in the tests: one PostgreSQL per test run, started by Testcontainers

### Changed
- `AGENTS.md` and `README.md`: the `Jwt:SigningKey` secret, the clock rule, demo sign-in and the fixed demo code
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
