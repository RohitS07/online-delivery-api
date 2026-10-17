using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Dtos;
using OnlineDelivery.Api.Entities;
using OnlineDelivery.Api.Services;
using OnlineDelivery.Api.Services.Auth;

namespace OnlineDelivery.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(OtpService otp, AppDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("otp")]
    public async Task<IActionResult> RequestCode(RequestOtpRequest request, CancellationToken cancellationToken) =>
        await otp.RequestAsync(request.MobileNumber, cancellationToken) switch
        {
            OtpRequestOutcome.Sent => Accepted(new RequestOtpResponse(
                (int)OtpService.ResendAfter.TotalSeconds, (int)OtpService.CodeLifetime.TotalSeconds)),
            OtpRequestOutcome.InvalidNumber => ApiProblems.Result(400, "mobile.invalid"),
            OtpRequestOutcome.TooSoon => ApiProblems.Result(429, "otp.tooSoon"),
            _ => ApiProblems.Result(429, "otp.tooManyRequests"),
        };

    [AllowAnonymous]
    [HttpPost("otp/verify")]
    public async Task<IActionResult> Verify(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await otp.VerifyAsync(request.MobileNumber, request.Code, cancellationToken);
        return result.Outcome switch
        {
            OtpVerifyOutcome.SignedIn => Ok(new SignedInResponse(
                "signedIn", result.Token!.Token, result.Token.ExpiresAt, ToResponse(result.User!))),
            OtpVerifyOutcome.RegistrationRequired => Ok(new RegistrationRequiredResponse(
                "registrationRequired", result.Token!.Token, result.Token.ExpiresAt, null)),
            OtpVerifyOutcome.InvalidNumber => ApiProblems.Result(400, "mobile.invalid"),
            OtpVerifyOutcome.InvalidCode => ApiProblems.Result(400, "otp.invalid"),
            OtpVerifyOutcome.Expired => ApiProblems.Result(400, "otp.expired"),
            OtpVerifyOutcome.TooManyAttempts => ApiProblems.Result(429, "otp.tooManyAttempts"),
            _ => ApiProblems.Result(403, "user.blocked"),
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var id = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == id, cancellationToken);
        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Role.ToString(), user.Status.ToString(), user.Name);
}
