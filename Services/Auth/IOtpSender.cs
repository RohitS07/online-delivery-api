namespace OnlineDelivery.Api.Services.Auth;

/// <summary>Sends a one-time code to a mobile number. Feature code never names a provider: ADR-0009.</summary>
public interface IOtpSender
{
    Task SendAsync(string mobileNumber, string code, CancellationToken cancellationToken);
}
