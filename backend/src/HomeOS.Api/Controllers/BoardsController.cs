using HomeOS.Application.Boards;
using HomeOS.Application.Boards.Commands;
using HomeOS.Application.Boards.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateBoardRequest(Guid HouseholdId, string Name);

[ApiController]
[Route("api/boards")]
[Authorize]
public class BoardsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BoardDto>>> Get([FromQuery] Guid householdId, CancellationToken ct)
    {
        var boards = await sender.Send(new GetBoardsQuery(householdId), ct);
        return Ok(boards);
    }

    [HttpPost]
    public async Task<ActionResult<BoardDto>> Create(CreateBoardRequest request, CancellationToken ct)
    {
        var board = await sender.Send(new CreateBoardCommand(request.HouseholdId, request.Name), ct);
        return Ok(board);
    }
}
