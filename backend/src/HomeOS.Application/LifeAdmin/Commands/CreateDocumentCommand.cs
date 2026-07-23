using HomeOS.Application.Common;
using HomeOS.Domain.LifeAdmin;
using MediatR;

namespace HomeOS.Application.LifeAdmin.Commands;

public record CreateDocumentCommand(Guid HouseholdId, string Title, string Category, Guid CreatedByUserId, DateTime? RenewalDateUtc, string? Notes) : IRequest<DocumentDto>;

public class CreateDocumentCommandHandler(IAppDbContext db) : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = HouseholdDocument.Create(request.HouseholdId, request.Title, request.Category, request.CreatedByUserId, request.RenewalDateUtc, request.Notes);
        db.Documents.Add(doc);
        await db.SaveChangesAsync(cancellationToken);
        return DocumentDto.From(doc);
    }
}
