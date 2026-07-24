using HomeOS.Application.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SearchResultDto>>> Get([FromQuery] Guid householdId, [FromQuery] string q, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetSearchResultsQuery(householdId, q), ct));
    }
}
