using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineDelivery.Api.Dtos;
using OnlineDelivery.Api.Services;

namespace OnlineDelivery.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/categories")]
public class CategoriesController(ReferenceDataService referenceData) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await referenceData.ListCategoriesAsync(cancellationToken));
}
