using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OnlineDelivery.Api.Services.Auth;

/// <summary>Token settings. The signing key is a secret, never in appsettings: ADR-0006.</summary>
public class JwtOptions : IValidatableObject
{
    public string SigningKey { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";

    public byte[] SigningKeyBytes => Encoding.UTF8.GetBytes(SigningKey);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SigningKeyBytes.Length < 32)
        {
            yield return new ValidationResult("Jwt:SigningKey must be set, in user-secrets or a secret manager, and be at least 32 bytes.");
        }

        if (string.IsNullOrEmpty(Issuer) || string.IsNullOrEmpty(Audience))
        {
            yield return new ValidationResult("Jwt:Issuer and Jwt:Audience must be set.");
        }
    }
}
