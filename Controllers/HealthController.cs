using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineDelivery.Api.Dtos;

namespace OnlineDelivery.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("healthy", "online-delivery-api", DateTime.UtcNow));
    }
}
