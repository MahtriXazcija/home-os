using System.Security.Claims;
using HomeOS.Application.Notes;
using HomeOS.Application.Notes.Commands;
using HomeOS.Application.Notes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateNoteRequest(Guid HouseholdId, string Content, string? Title, List<string>? Tags, DateOnly? JournalDate, string? LinkedType, Guid? LinkedId);
public record UpdateNoteRequest(string Content, string? Title, List<string>? Tags);

[ApiController]
[Route("api/notes")]
[Authorize]
public class NotesController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<NoteDto>>> Get([FromQuery] Guid householdId, CancellationToken ct)
    {
        var notes = await sender.Send(new GetNotesQuery(householdId), ct);
        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create(CreateNoteRequest request, CancellationToken ct)
    {
        var note = await sender.Send(new CreateNoteCommand(
            request.HouseholdId, request.Content, CurrentUserId, request.Title, request.Tags ?? [], request.JournalDate, request.LinkedType, request.LinkedId), ct);
        return Ok(note);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoteDto>> Update(Guid id, UpdateNoteRequest request, CancellationToken ct)
    {
        var note = await sender.Send(new UpdateNoteCommand(id, request.Content, request.Title, request.Tags ?? []), ct);
        return Ok(note);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteNoteCommand(id), ct);
        return NoContent();
    }
}
