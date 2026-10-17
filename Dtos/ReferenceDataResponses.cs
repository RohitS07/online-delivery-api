namespace OnlineDelivery.Api.Dtos;

/// <summary>The bodies of the reference-data endpoints: SPEC-0002.</summary>
public sealed record CityResponse(int Id, string NameEn, string NameMr, IReadOnlyList<VillageResponse> Villages);

public sealed record VillageResponse(int Id, string NameEn, string NameMr, int DeliveryHours);

public sealed record CategoryResponse(int Id, string NameEn, string NameMr);
