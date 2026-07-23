using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Queries;

public record GetContactsQuery(Guid HouseholdId) : IRequest<List<ContactDto>>;

public class GetContactsQueryHandler(IAppDbContext db) : IRequestHandler<GetContactsQuery, List<ContactDto>>
{
    public async Task<List<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await db.Contacts
            .Where(c => c.HouseholdId == request.HouseholdId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return contacts.Select(ContactDto.From).ToList();
    }
}
