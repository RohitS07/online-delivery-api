using System.Security.Cryptography;

namespace OnlineDelivery.Api.Services.Auth;

/// <summary>Six random digits from a cryptographic generator: SPEC-0003.</summary>
public class OtpCodeGenerator(IHostEnvironment environment, IConfiguration configuration)
{
    public string Next()
    {
        // A fixed code for demos, honoured only in Development: ADR-0009.
        var fixedCode = configuration["Otp:Mock:FixedCode"];
        if (environment.IsDevelopment() && !string.IsNullOrEmpty(fixedCode))
        {
            return fixedCode;
        }

        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }
}
