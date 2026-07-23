using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notes.Commands;

public record DeleteNoteCommand(Guid NoteId) : IRequest;

public class DeleteNoteCommandHandler(IAppDbContext db) : IRequestHandler<DeleteNoteCommand>
{
    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);
        if (note is null) return;

        db.Notes.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
    }
}
