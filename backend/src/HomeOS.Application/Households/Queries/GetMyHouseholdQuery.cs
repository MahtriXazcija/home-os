using MediatR;

namespace HomeOS.Application.Households.Queries;

public record GetMyHouseholdQuery(Guid UserId) : IRequest<HouseholdDto?>;

public class GetMyHouseholdQueryHandler(IHouseholdRepository repository) : IRequestHandler<GetMyHouseholdQuery, HouseholdDto?>
{
    public async Task<HouseholdDto?> Handle(GetMyHouseholdQuery request, CancellationToken cancellationToken)
    {
        var household = await repository.GetByMemberUserIdAsync(request.UserId, cancellationToken);
        return household is null ? null : HouseholdDto.From(household);
    }
}
