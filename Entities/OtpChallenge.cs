namespace OnlineDelivery.Api.Entities;

/// <summary>One code sent to a mobile number. Only its hash is stored: SPEC-0003.</summary>
public class OtpChallenge
{
    public Guid Id { get; set; }
    public required string MobileNumber { get; set; }
    public required string CodeHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
