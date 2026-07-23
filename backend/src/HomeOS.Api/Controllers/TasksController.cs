using System.Security.Claims;
using HomeOS.Application.Tasks;
using HomeOS.Application.Tasks.Commands;
using HomeOS.Application.Tasks.Queries;
using HomeOS.Domain.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateTaskRequest(
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority,
    Guid? AssignedToUserId,
    Guid? BoardId,
    Guid? ParentTaskId,
    RecurrenceRule Recurrence,
    List<string>? Tags);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority,
    Guid? AssignedToUserId,
    RecurrenceRule Recurrence,
    List<string>? Tags);

public record ChangeStatusRequest(HomeTaskStatus Status);

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> Get([FromQuery] Guid householdId, [FromQuery] Guid? boardId, CancellationToken ct)
    {
        var tasks = await sender.Send(new GetTasksQuery(householdId, boardId), ct);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var task = await sender.Send(new CreateTaskCommand(
            request.HouseholdId, request.Title, CurrentUserId, request.Description, request.DueDateUtc,
            request.Priority, request.AssignedToUserId, request.BoardId, request.ParentTaskId,
            request.Recurrence, request.Tags ?? []), ct);
        return Ok(task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        try
        {
            var task = await sender.Send(new UpdateTaskCommand(
                id, request.Title, request.Description, request.DueDateUtc, request.Priority,
                request.AssignedToUserId, request.Recurrence, request.Tags ?? []), ct);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskDto>> ChangeStatus(Guid id, ChangeStatusRequest request, CancellationToken ct)
    {
        try
        {
            var task = await sender.Send(new ChangeTaskStatusCommand(id, request.Status), ct);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<TaskDto>> Complete(Guid id, CancellationToken ct)
    {
        try
        {
            var task = await sender.Send(new CompleteTaskCommand(id, CurrentUserId), ct);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<TaskDto>> Reopen(Guid id, CancellationToken ct)
    {
        try
        {
            var task = await sender.Send(new ReopenTaskCommand(id), ct);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteTaskCommand(id), ct);
        return NoContent();
    }
}
