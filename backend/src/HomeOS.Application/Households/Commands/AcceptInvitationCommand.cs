using MediatR;

namespace HomeOS.Application.Households.Commands;

public record AcceptInvitationCommand(string Token, Guid UserId, string DisplayName) : IRequest<HouseholdDto>;

public class AcceptInvitationCommandHandler(IHouseholdRepository repository) : IRequestHandler<AcceptInvitationCommand, HouseholdDto>
{
    public async Task<HouseholdDto> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var household = await repository.GetByInvitationTokenAsync(request.Token, cancellationToken)
            ?? throw new InvalidOperationException("Invitation not found.");

        var existingHousehold = await repository.GetByMemberUserIdAsync(request.UserId, cancellationToken);
        if (existingHousehold is not null)
        {
            throw new InvalidOperationException("You already belong to a household.");
        }

        household.AcceptInvitation(request.Token, request.UserId, request.DisplayName);
        await repository.SaveChangesAsync(cancellationToken);

        return HouseholdDto.From(household);
    }
}
