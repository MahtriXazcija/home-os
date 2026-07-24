using MediatR;

namespace HomeOS.Application.Households.Commands;

public record RemoveMemberCommand(Guid HouseholdId, Guid UserId, Guid RemovedByUserId) : IRequest;

public class RemoveMemberCommandHandler(IHouseholdRepository repository) : IRequestHandler<RemoveMemberCommand>
{
    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var household = await repository.GetByIdAsync(request.HouseholdId, cancellationToken)
            ?? throw new InvalidOperationException("Household not found.");

        household.RemoveMember(request.UserId, request.RemovedByUserId);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
