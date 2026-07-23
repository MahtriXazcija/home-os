using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notes.Commands;

public record UpdateNoteCommand(Guid NoteId, string Content, string? Title, List<string> Tags) : IRequest<NoteDto>;

public class UpdateNoteCommandHandler(IAppDbContext db) : IRequestHandler<UpdateNoteCommand, NoteDto>
{
    public async Task<NoteDto> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken)
            ?? throw new InvalidOperationException("Note not found.");

        note.Update(request.Content, request.Title, request.Tags);
        await db.SaveChangesAsync(cancellationToken);

        return NoteDto.From(note);
    }
}
