namespace OnlineDelivery.Api.Dtos;

/// <summary>Request and response bodies of the sign-in endpoints: SPEC-0003.</summary>
public sealed record RequestOtpRequest(string? MobileNumber);

public sealed record RequestOtpResponse(int ResendAfterSeconds, int ExpiresInSeconds);

public sealed record VerifyOtpRequest(string? MobileNumber, string? Code);

public sealed record UserResponse(Guid Id, string Role, string Status, string Name);

public sealed record SignedInResponse(string Result, string AccessToken, DateTime ExpiresAt, UserResponse User);

public sealed record RegistrationRequiredResponse(string Result, string RegistrationToken, DateTime ExpiresAt, UserResponse? User);
