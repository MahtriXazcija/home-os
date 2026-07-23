using HomeOS.Domain.Common;

namespace HomeOS.Domain.LifeAdmin;

public class Contact : Entity
{
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Contact() { }

    public static Contact Create(Guid householdId, string name, Guid createdByUserId, string? phone = null, string? email = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new Contact
        {
            HouseholdId = householdId,
            Name = name.Trim(),
            Phone = phone,
            Email = email,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
