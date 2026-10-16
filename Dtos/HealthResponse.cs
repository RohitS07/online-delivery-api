namespace OnlineDelivery.Api.Dtos;

/// <summary>The body of <c>GET /api/v1/health</c>: SPEC-0001.</summary>
public sealed record HealthResponse(string Status, string Service, DateTime Timestamp);
