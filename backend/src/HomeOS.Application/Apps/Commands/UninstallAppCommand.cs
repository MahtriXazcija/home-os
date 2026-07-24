using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Apps.Commands;

public record UninstallAppCommand(Guid HouseholdId, string AppId) : IRequest;

public class UninstallAppCommandHandler(IAppDbContext db) : IRequestHandler<UninstallAppCommand>
{
    public async Task Handle(UninstallAppCommand request, CancellationToken cancellationToken)
    {
        var manifest = AppCatalog.Find(request.AppId);
        if (manifest is { IsCore: true })
        {
            throw new InvalidOperationException($"{manifest.Name} is core and can't be uninstalled.");
        }

        var installation = await db.AppInstallations
            .FirstOrDefaultAsync(a => a.HouseholdId == request.HouseholdId && a.AppId == request.AppId, cancellationToken);
        if (installation is null) return;

        db.AppInstallations.Remove(installation);
        await db.SaveChangesAsync(cancellationToken);
    }
}
