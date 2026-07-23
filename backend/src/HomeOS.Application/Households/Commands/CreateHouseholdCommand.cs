using HomeOS.Domain.Households;
using MediatR;

namespace HomeOS.Application.Households.Commands;

public record CreateHouseholdCommand(string Name, Guid UserId, string UserDisplayName) : IRequest<HouseholdDto>;

public class CreateHouseholdCommandHandler(IHouseholdRepository repository) : IRequestHandler<CreateHouseholdCommand, HouseholdDto>
{
    public async Task<HouseholdDto> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByMemberUserIdAsync(request.UserId, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("You already belong to a household.");
        }

        var household = Household.Create(request.Name, request.UserId, request.UserDisplayName);
        repository.Add(household);
        await repository.SaveChangesAsync(cancellationToken);

        return HouseholdDto.From(household);
    }
}
