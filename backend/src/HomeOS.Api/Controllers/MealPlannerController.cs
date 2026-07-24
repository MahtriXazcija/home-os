using System.Security.Claims;
using HomeOS.Application.MealPlanner;
using HomeOS.Application.MealPlanner.Commands;
using HomeOS.Application.MealPlanner.Queries;
using HomeOS.Domain.MealPlanner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateMealPlanEntryRequest(Guid HouseholdId, DateOnly MealDate, MealType MealType, string Title, bool AddShoppingTask);

[ApiController]
[Route("api/meal-planner")]
[Authorize]
public class MealPlannerController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<MealPlanEntryDto>>> Get([FromQuery] Guid householdId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetMealPlanQuery(householdId, fromDate, toDate), ct));
    }

    [HttpPost]
    public async Task<ActionResult<MealPlanEntryDto>> Create(CreateMealPlanEntryRequest request, CancellationToken ct)
    {
        var entry = await sender.Send(new CreateMealPlanEntryCommand(
            request.HouseholdId, request.MealDate, request.MealType, request.Title, CurrentUserId, request.AddShoppingTask), ct);
        return Ok(entry);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteMealPlanEntryCommand(id), ct);
        return NoContent();
    }
}
