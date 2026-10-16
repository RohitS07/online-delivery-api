# Changelog — online-delivery-api

Newest first. See [RULE-0016](../harness/rules/RULE-0016-changelog-format.md).

## 2026-10-16

### Added
- `AGENTS.md`, with `CLAUDE.md` linked to it, routing to the workspace harness
- User secrets, holding the connection string; `.env.example` for Docker Compose

### Changed
- CI's test step passes when there is no `tests/` folder yet
- The PostgreSQL password comes from `.env` and user-secrets, never a tracked file (RULE-0014)
- `OnlineDelivery.Api.http` and `README.md` use port 5154 and the health endpoint

### Removed
- The `WeatherForecast` template controller and model
