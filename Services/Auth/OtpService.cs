using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Services.Auth;

public enum OtpRequestOutcome
{
    Sent,
    InvalidNumber,
    TooSoon,
    TooManyRequests,
}

public enum OtpVerifyOutcome
{
    SignedIn,
    RegistrationRequired,
    InvalidNumber,
    InvalidCode,
    Expired,
    TooManyAttempts,
    Blocked,
}

public sealed record OtpVerifyResult(OtpVerifyOutcome Outcome, IssuedToken? Token = null, User? User = null);

/// <summary>Sends and checks one-time codes, within the limits of SPEC-0003.</summary>
public class OtpService(
    AppDbContext db,
    IOtpSender sender,
    OtpCodeGenerator codes,
    TokenService tokens,
    IOptions<JwtOptions> jwt,
    TimeProvider time)
{
    public static readonly TimeSpan ResendAfter = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);
    private const int MaxCodesPerHour = 5;
    private const int MaxAttempts = 5;

    public async Task<OtpRequestOutcome> RequestAsync(string? input, CancellationToken cancellationToken)
    {
        if (!MobileNumber.TryNormalize(input, out var number))
        {
            return OtpRequestOutcome.InvalidNumber;
        }

        // One request per number at a time, so the limits hold under parallel requests: SPEC-0003.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({number}))", cancellationToken);

        var now = time.GetUtcNow();
        var lastHour = await db.OtpChallenges
            .Where(c => c.MobileNumber == number && c.CreatedAt > now.AddHours(-1))
            .ToListAsync(cancellationToken);
        if (lastHour.Any(c => c.CreatedAt > now - ResendAfter))
        {
            return OtpRequestOutcome.TooSoon;
        }

        if (lastHour.Count >= MaxCodesPerHour)
        {
            return OtpRequestOutcome.TooManyRequests;
        }

        // A new code replaces any earlier one: SPEC-0003.
        await db.OtpChallenges
            .Where(c => c.MobileNumber == number && c.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedAt, now), cancellationToken);

        var code = codes.Next();
        var challenge = new OtpChallenge
        {
            Id = Guid.NewGuid(),
            MobileNumber = number,
            CodeHash = "",
            ExpiresAt = now + CodeLifetime,
            CreatedAt = now,
        };
        challenge.CodeHash = Hash(challenge.Id, code);
        db.OtpChallenges.Add(challenge);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await sender.SendAsync(number, code, cancellationToken);
        return OtpRequestOutcome.Sent;
    }

    public async Task<OtpVerifyResult> VerifyAsync(string? input, string? code, CancellationToken cancellationToken)
    {
        if (!MobileNumber.TryNormalize(input, out var number))
        {
            return new OtpVerifyResult(OtpVerifyOutcome.InvalidNumber);
        }

        var now = time.GetUtcNow();
        var challenge = await db.OtpChallenges.AsNoTracking()
            .Where(c => c.MobileNumber == number && c.UsedAt == null && c.ExpiresAt > now && c.FailedAttempts < MaxAttempts)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (challenge is null)
        {
            return new OtpVerifyResult(OtpVerifyOutcome.Expired);
        }

        // Each check takes one of the 5 attempts atomically before comparing, so parallel guesses
        // cannot exceed the limit: SPEC-0003. A right code also takes one, and is then used.
        var attempt = await db.Database.SqlQuery<int>(
            $"UPDATE \"OtpChallenges\" SET \"FailedAttempts\" = \"FailedAttempts\" + 1 WHERE \"Id\" = {challenge.Id} AND \"UsedAt\" IS NULL AND \"ExpiresAt\" > {now} AND \"FailedAttempts\" < {MaxAttempts} RETURNING \"FailedAttempts\" AS \"Value\"")
            .ToListAsync(cancellationToken);
        if (attempt.Count == 0)
        {
            return new OtpVerifyResult(OtpVerifyOutcome.Expired);
        }

        if (!Matches(challenge, code))
        {
            return new OtpVerifyResult(attempt[0] >= MaxAttempts ? OtpVerifyOutcome.TooManyAttempts : OtpVerifyOutcome.InvalidCode);
        }

        // Single use: only one request can claim the code: SPEC-0003.
        var claimed = await db.OtpChallenges
            .Where(c => c.Id == challenge.Id && c.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedAt, now), cancellationToken);
        if (claimed == 0)
        {
            return new OtpVerifyResult(OtpVerifyOutcome.Expired);
        }

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.MobileNumber == number, cancellationToken);
        if (user is null)
        {
            return new OtpVerifyResult(OtpVerifyOutcome.RegistrationRequired, tokens.CreateRegistrationToken(number));
        }

        if (user.Status == UserStatus.Blocked)
        {
            return new OtpVerifyResult(OtpVerifyOutcome.Blocked);
        }

        return new OtpVerifyResult(OtpVerifyOutcome.SignedIn, tokens.CreateAccessToken(user), user);
    }

    private bool Matches(OtpChallenge challenge, string? code) =>
        code is not null && CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Hash(challenge.Id, code)), Encoding.ASCII.GetBytes(challenge.CodeHash));

    // Keyed and labelled, so a copy of the database alone cannot be tried against all million codes: SPEC-0003.
    private string Hash(Guid challengeId, string code) =>
        Convert.ToHexString(HMACSHA256.HashData(
            jwt.Value.SigningKeyBytes, Encoding.UTF8.GetBytes($"otp-code:{challengeId}:{code}")));
}
