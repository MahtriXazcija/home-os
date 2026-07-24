using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Apps.Commands;

public record InstallAppCommand(Guid HouseholdId, string AppId, List<string> GrantedPermissions) : IRequest<AppDto>;

public class InstallAppCommandHandler(IAppDbContext db) : IRequestHandler<InstallAppCommand, AppDto>
{
    public async Task<AppDto> Handle(InstallAppCommand request, CancellationToken cancellationToken)
    {
        var manifest = AppCatalog.Find(request.AppId)
            ?? throw new InvalidOperationException("Unknown app id — not in the catalog.");

        // The household only ever grants what the manifest actually asks for,
        // even if the client sent more (docs/app-sdk.md §4).
        var grantable = request.GrantedPermissions.Intersect(manifest.Permissions).ToList();

        var existing = await db.AppInstallations
            .FirstOrDefaultAsync(a => a.HouseholdId == request.HouseholdId && a.AppId == request.AppId, cancellationToken);
        if (existing is not null)
        {
            return AppDto.From(manifest, existing);
        }

        var installation = AppInstallation.Create(request.HouseholdId, request.AppId, grantable);
        db.AppInstallations.Add(installation);
        await db.SaveChangesAsync(cancellationToken);

        return AppDto.From(manifest, installation);
    }
}
