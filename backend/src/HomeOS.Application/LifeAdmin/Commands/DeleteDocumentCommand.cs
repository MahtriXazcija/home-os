using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Commands;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest;

public class DeleteDocumentCommandHandler(IAppDbContext db) : IRequestHandler<DeleteDocumentCommand>
{
    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
        if (doc is null) return;

        db.Documents.Remove(doc);
        await db.SaveChangesAsync(cancellationToken);
    }
}
