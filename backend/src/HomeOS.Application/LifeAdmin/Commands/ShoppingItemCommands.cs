using HomeOS.Application.Common;
using HomeOS.Domain.LifeAdmin;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Commands;

public record AddShoppingItemCommand(Guid HouseholdId, string Text, Guid CreatedByUserId) : IRequest<ShoppingItemDto>;
public record SetShoppingItemCheckedCommand(Guid ItemId, bool IsChecked) : IRequest;
public record DeleteShoppingItemCommand(Guid ItemId) : IRequest;

public class AddShoppingItemCommandHandler(IAppDbContext db) : IRequestHandler<AddShoppingItemCommand, ShoppingItemDto>
{
    public async Task<ShoppingItemDto> Handle(AddShoppingItemCommand request, CancellationToken cancellationToken)
    {
        var item = ShoppingItem.Create(request.HouseholdId, request.Text, request.CreatedByUserId);
        db.ShoppingItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ShoppingItemDto.From(item);
    }
}

public class SetShoppingItemCheckedCommandHandler(IAppDbContext db) : IRequestHandler<SetShoppingItemCheckedCommand>
{
    public async Task Handle(SetShoppingItemCheckedCommand request, CancellationToken cancellationToken)
    {
        var item = await db.ShoppingItems.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
        if (item is null) return;

        item.SetChecked(request.IsChecked);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteShoppingItemCommandHandler(IAppDbContext db) : IRequestHandler<DeleteShoppingItemCommand>
{
    public async Task Handle(DeleteShoppingItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.ShoppingItems.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
        if (item is null) return;

        db.ShoppingItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
    }
}
