using HomeOS.Application.Common;
using HomeOS.Domain.LifeAdmin;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Commands;

public record CreateContactCommand(Guid HouseholdId, string Name, Guid CreatedByUserId, string? Phone, string? Email, string? Notes) : IRequest<ContactDto>;
public record DeleteContactCommand(Guid ContactId) : IRequest;

public class CreateContactCommandHandler(IAppDbContext db) : IRequestHandler<CreateContactCommand, ContactDto>
{
    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = Contact.Create(request.HouseholdId, request.Name, request.CreatedByUserId, request.Phone, request.Email, request.Notes);
        db.Contacts.Add(contact);
        await db.SaveChangesAsync(cancellationToken);
        return ContactDto.From(contact);
    }
}

public class DeleteContactCommandHandler(IAppDbContext db) : IRequestHandler<DeleteContactCommand>
{
    public async Task Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await db.Contacts.FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);
        if (contact is null) return;

        db.Contacts.Remove(contact);
        await db.SaveChangesAsync(cancellationToken);
    }
}
