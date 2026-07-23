using HomeOS.Application.Common;
using HomeOS.Domain.Notes;
using MediatR;

namespace HomeOS.Application.Notes.Commands;

public record CreateNoteCommand(
    Guid HouseholdId, string Content, Guid CreatedByUserId,
    string? Title, List<string> Tags, DateOnly? JournalDate, string? LinkedType, Guid? LinkedId) : IRequest<NoteDto>;

public class CreateNoteCommandHandler(IAppDbContext db) : IRequestHandler<CreateNoteCommand, NoteDto>
{
    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = Note.Create(
            request.HouseholdId, request.Content, request.CreatedByUserId,
            request.Title, request.Tags, request.JournalDate, request.LinkedType, request.LinkedId);

        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        return NoteDto.From(note);
    }
}
