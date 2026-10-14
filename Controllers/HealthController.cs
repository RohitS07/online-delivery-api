using Microsoft.AspNetCore.Mvc;

namespace OnlineDelivery.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "online-delivery-api",
            timestamp = DateTime.UtcNow
        });
    }
}