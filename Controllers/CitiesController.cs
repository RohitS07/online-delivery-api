using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineDelivery.Api.Dtos;
using OnlineDelivery.Api.Services;

namespace OnlineDelivery.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/cities")]
public class CitiesController(ReferenceDataService referenceData) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CityResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await referenceData.ListCitiesAsync(cancellationToken));
}
