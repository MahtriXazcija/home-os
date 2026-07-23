using HomeOS.Domain.LifeAdmin;

namespace HomeOS.Application.LifeAdmin;

public record DocumentDto(Guid Id, Guid HouseholdId, string Title, string Category, DateTime? RenewalDateUtc, string? Notes, Guid CreatedByUserId, DateTime CreatedAtUtc)
{
    public static DocumentDto From(HouseholdDocument d) => new(d.Id, d.HouseholdId, d.Title, d.Category, d.RenewalDateUtc, d.Notes, d.CreatedByUserId, d.CreatedAtUtc);
}

public record ContactDto(Guid Id, string Name, string? Phone, string? Email, string? Notes)
{
    public static ContactDto From(Contact c) => new(c.Id, c.Name, c.Phone, c.Email, c.Notes);
}

public record ShoppingItemDto(Guid Id, string Text, bool IsChecked)
{
    public static ShoppingItemDto From(ShoppingItem s) => new(s.Id, s.Text, s.IsChecked);
}
