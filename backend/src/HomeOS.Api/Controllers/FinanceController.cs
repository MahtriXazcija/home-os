using System.Security.Claims;
using HomeOS.Application.Finance;
using HomeOS.Application.Finance.Commands;
using HomeOS.Application.Finance.Queries;
using HomeOS.Domain.Finance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateTransactionRequest(Guid HouseholdId, TransactionType Type, string Category, decimal Amount, DateTime OccurredAtUtc, string? Description);
public record CreateBillRequest(Guid HouseholdId, string Title, decimal Amount, string Category, DateTime DueDateUtc, BillRecurrence Recurrence);
public record SetBudgetRequest(Guid HouseholdId, string Category, decimal MonthlyLimit);

[ApiController]
[Route("api/finance")]
[Authorize]
public class FinanceController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactions([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetTransactionsQuery(householdId), ct));
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionRequest request, CancellationToken ct)
    {
        var transaction = await sender.Send(new CreateTransactionCommand(
            request.HouseholdId, request.Type, request.Category, request.Amount, request.OccurredAtUtc, CurrentUserId, request.Description), ct);
        return Ok(transaction);
    }

    [HttpDelete("transactions/{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteTransactionCommand(id), ct);
        return NoContent();
    }

    [HttpGet("bills")]
    public async Task<ActionResult<List<BillDto>>> GetBills([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetBillsQuery(householdId), ct));
    }

    [HttpPost("bills")]
    public async Task<ActionResult<BillDto>> CreateBill(CreateBillRequest request, CancellationToken ct)
    {
        var bill = await sender.Send(new CreateBillCommand(
            request.HouseholdId, request.Title, request.Amount, request.Category, request.DueDateUtc, CurrentUserId, request.Recurrence), ct);
        return Ok(bill);
    }

    [HttpPost("bills/{id:guid}/pay")]
    public async Task<ActionResult<BillDto>> PayBill(Guid id, CancellationToken ct)
    {
        var bill = await sender.Send(new MarkBillPaidCommand(id, CurrentUserId), ct);
        return Ok(bill);
    }

    [HttpPut("budgets")]
    public async Task<IActionResult> SetBudget(SetBudgetRequest request, CancellationToken ct)
    {
        await sender.Send(new SetBudgetCommand(request.HouseholdId, request.Category, request.MonthlyLimit), ct);
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryDto>> GetSummary([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetFinanceSummaryQuery(householdId), ct));
    }
}
