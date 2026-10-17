using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Services.Auth;

public sealed record IssuedToken(string Token, DateTime ExpiresAt);

/// <summary>Access tokens for 7 days, registration tokens for 15 minutes: SPEC-0003.</summary>
public class TokenService(IOptions<JwtOptions> options, TimeProvider time)
{
    public const string PurposeClaim = "purpose";
    public const string RegistrationPurpose = "registration";
    public const string MobileNumberClaim = "mobile_number";

    private static readonly TimeSpan AccessLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan RegistrationLifetime = TimeSpan.FromMinutes(15);

    public IssuedToken CreateAccessToken(User user) =>
        Create(AccessLifetime, new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            ["role"] = user.Role.ToString(),
        });

    public IssuedToken CreateRegistrationToken(string mobileNumber) =>
        Create(RegistrationLifetime, new Dictionary<string, object>
        {
            [PurposeClaim] = RegistrationPurpose,
            [MobileNumberClaim] = mobileNumber,
        });

    private IssuedToken Create(TimeSpan lifetime, Dictionary<string, object> claims)
    {
        var jwt = options.Value;
        var now = time.GetUtcNow().UtcDateTime;
        var expires = now + lifetime;
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Claims = claims,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(jwt.SigningKeyBytes), SecurityAlgorithms.HmacSha256),
        });
        return new IssuedToken(token, expires);
    }
}
