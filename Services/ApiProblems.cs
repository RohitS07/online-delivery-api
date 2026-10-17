using Microsoft.AspNetCore.Mvc;

namespace OnlineDelivery.Api.Services;

/// <summary>Errors are ProblemDetails with a stable code the app translates: ADR-0010.</summary>
public static class ApiProblems
{
    public static ProblemDetails Create(int status, string code)
    {
        var problem = new ProblemDetails { Status = status, Title = code };
        problem.Extensions["code"] = code;
        return problem;
    }

    public static ObjectResult Result(int status, string code) =>
        new(Create(status, code)) { StatusCode = status, ContentTypes = { "application/problem+json" } };

    public static Task WriteAsync(HttpContext context, int status, string code)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(Create(status, code), options: null, contentType: "application/problem+json");
    }
}
