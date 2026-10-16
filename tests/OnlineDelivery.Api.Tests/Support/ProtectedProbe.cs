using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OnlineDelivery.Api.Tests.Support;

/// <summary>A test-only endpoint that requires authorization, for AC-0001-04.</summary>
public static class ProtectedProbe
{
    public const string Route = "/api/v1/test/protected";
    private const string Scheme = "NoUser";

    public static void Register(IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(ProtectedProbe).Assembly);
        services.AddAuthentication(Scheme).AddScheme<AuthenticationSchemeOptions, NoUserHandler>(Scheme, _ => { });
    }

    private sealed class NoUserHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());
    }
}

[ApiController]
[Authorize]
[Route(ProtectedProbe.Route)]
public class ProtectedProbeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
