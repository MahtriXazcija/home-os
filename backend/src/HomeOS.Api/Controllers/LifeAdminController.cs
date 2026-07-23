using System.Security.Claims;
using HomeOS.Application.LifeAdmin;
using HomeOS.Application.LifeAdmin.Commands;
using HomeOS.Application.LifeAdmin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateDocumentRequest(Guid HouseholdId, string Title, string Category, DateTime? RenewalDateUtc, string? Notes);
public record CreateContactRequest(Guid HouseholdId, string Name, string? Phone, string? Email, string? Notes);
public record AddShoppingItemRequest(Guid HouseholdId, string Text);
public record SetCheckedRequest(bool IsChecked);

[ApiController]
[Route("api/life-admin")]
[Authorize]
public class LifeAdminController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("documents")]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetDocumentsQuery(householdId), ct));
    }

    [HttpPost("documents")]
    public async Task<ActionResult<DocumentDto>> CreateDocument(CreateDocumentRequest request, CancellationToken ct)
    {
        var doc = await sender.Send(new CreateDocumentCommand(request.HouseholdId, request.Title, request.Category, CurrentUserId, request.RenewalDateUtc, request.Notes), ct);
        return Ok(doc);
    }

    [HttpDelete("documents/{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteDocumentCommand(id), ct);
        return NoContent();
    }

    [HttpGet("contacts")]
    public async Task<ActionResult<List<ContactDto>>> GetContacts([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetContactsQuery(householdId), ct));
    }

    [HttpPost("contacts")]
    public async Task<ActionResult<ContactDto>> CreateContact(CreateContactRequest request, CancellationToken ct)
    {
        var contact = await sender.Send(new CreateContactCommand(request.HouseholdId, request.Name, CurrentUserId, request.Phone, request.Email, request.Notes), ct);
        return Ok(contact);
    }

    [HttpDelete("contacts/{id:guid}")]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteContactCommand(id), ct);
        return NoContent();
    }

    [HttpGet("shopping-items")]
    public async Task<ActionResult<List<ShoppingItemDto>>> GetShoppingItems([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetShoppingItemsQuery(householdId), ct));
    }

    [HttpPost("shopping-items")]
    public async Task<ActionResult<ShoppingItemDto>> AddShoppingItem(AddShoppingItemRequest request, CancellationToken ct)
    {
        var item = await sender.Send(new AddShoppingItemCommand(request.HouseholdId, request.Text, CurrentUserId), ct);
        return Ok(item);
    }

    [HttpPatch("shopping-items/{id:guid}")]
    public async Task<IActionResult> SetShoppingItemChecked(Guid id, SetCheckedRequest request, CancellationToken ct)
    {
        await sender.Send(new SetShoppingItemCheckedCommand(id, request.IsChecked), ct);
        return NoContent();
    }

    [HttpDelete("shopping-items/{id:guid}")]
    public async Task<IActionResult> DeleteShoppingItem(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteShoppingItemCommand(id), ct);
        return NoContent();
    }
}
