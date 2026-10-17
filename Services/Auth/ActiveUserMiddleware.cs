using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Services.Auth;

/// <summary>A token cannot be taken back, so each authorized request checks the user: ADR-0006.</summary>
public class ActiveUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();
        var needsUser = endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null
            && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null;
        if (needsUser && context.User.Identity?.IsAuthenticated == true)
        {
            var user = Guid.TryParse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
                ? await db.Users.AsNoTracking().Where(u => u.Id == id)
                    .Select(u => new { u.Role, u.Status }).SingleOrDefaultAsync()
                : null;

            if (user?.Status == UserStatus.Blocked)
            {
                await ApiProblems.WriteAsync(context, StatusCodes.Status403Forbidden, "user.blocked");
                return;
            }

            // A token whose user is gone, or whose role has changed, no longer works.
            if (user is null || user.Role.ToString() != context.User.FindFirst("role")?.Value)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(context);
    }
}
