using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.LifeAdmin.Queries;

public record GetDocumentsQuery(Guid HouseholdId) : IRequest<List<DocumentDto>>;

public class GetDocumentsQueryHandler(IAppDbContext db) : IRequestHandler<GetDocumentsQuery, List<DocumentDto>>
{
    public async Task<List<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await db.Documents
            .Where(d => d.HouseholdId == request.HouseholdId)
            .OrderBy(d => d.RenewalDateUtc)
            .ToListAsync(cancellationToken);

        return docs.Select(DocumentDto.From).ToList();
    }
}
