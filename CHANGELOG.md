# Changelog — online-delivery-api

Newest first. See [RULE-0016](../harness/rules/RULE-0016-changelog-format.md).

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
- The `WeatherForecast` template controller and model
