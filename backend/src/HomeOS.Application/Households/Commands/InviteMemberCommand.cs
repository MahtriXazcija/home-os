using MediatR;

namespace HomeOS.Application.Households.Commands;

public record InviteMemberCommand(Guid HouseholdId, string Email, Guid InvitedByUserId) : IRequest<InvitationDto>;

public class InviteMemberCommandHandler(IHouseholdRepository repository) : IRequestHandler<InviteMemberCommand, InvitationDto>
{
    public async Task<InvitationDto> Handle(InviteMemberCommand request, CancellationToken cancellationToken)
    {
        var household = await repository.GetByIdAsync(request.HouseholdId, cancellationToken)
            ?? throw new InvalidOperationException("Household not found.");

        var invitation = household.InviteMember(request.Email, request.InvitedByUserId);
        await repository.SaveChangesAsync(cancellationToken);

        return InvitationDto.From(invitation);
    }
}
