using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Apps.Queries;

public record GetHouseholdAppsQuery(Guid HouseholdId) : IRequest<List<AppDto>>;

public class GetHouseholdAppsQueryHandler(IAppDbContext db) : IRequestHandler<GetHouseholdAppsQuery, List<AppDto>>
{
    public async Task<List<AppDto>> Handle(GetHouseholdAppsQuery request, CancellationToken cancellationToken)
    {
        var installations = await db.AppInstallations
            .Where(a => a.HouseholdId == request.HouseholdId)
            .ToDictionaryAsync(a => a.AppId, cancellationToken);

        return AppCatalog.All
            .Select(manifest => AppDto.From(manifest, installations.GetValueOrDefault(manifest.Id)))
            .ToList();
    }
}
