using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Queries;

public record GetShoppingItemsQuery(Guid HouseholdId) : IRequest<List<ShoppingItemDto>>;

public class GetShoppingItemsQueryHandler(IAppDbContext db) : IRequestHandler<GetShoppingItemsQuery, List<ShoppingItemDto>>
{
    public async Task<List<ShoppingItemDto>> Handle(GetShoppingItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await db.ShoppingItems
            .Where(i => i.HouseholdId == request.HouseholdId)
            .OrderBy(i => i.IsChecked).ThenByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return items.Select(ShoppingItemDto.From).ToList();
    }
}
