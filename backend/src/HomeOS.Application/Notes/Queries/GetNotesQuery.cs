using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notes.Queries;

public record GetNotesQuery(Guid HouseholdId) : IRequest<List<NoteDto>>;

public class GetNotesQueryHandler(IAppDbContext db) : IRequestHandler<GetNotesQuery, List<NoteDto>>
{
    public async Task<List<NoteDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await db.Notes
            .Where(n => n.HouseholdId == request.HouseholdId)
            .OrderByDescending(n => n.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return notes.Select(NoteDto.From).ToList();
    }
}
