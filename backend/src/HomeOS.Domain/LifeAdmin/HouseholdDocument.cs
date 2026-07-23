using HomeOS.Domain.Common;

namespace HomeOS.Domain.LifeAdmin;

/// <summary>Named HouseholdDocument, not Document, to avoid clashing with System.Xml/PDF-library Document types.</summary>
public class HouseholdDocument : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DateTime? RenewalDateUtc { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private HouseholdDocument() { }

    public static HouseholdDocument Create(Guid householdId, string title, string category, Guid createdByUserId, DateTime? renewalDateUtc = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        var doc = new HouseholdDocument
        {
            HouseholdId = householdId,
            Title = title.Trim(),
            Category = category.Trim(),
            RenewalDateUtc = renewalDateUtc,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (renewalDateUtc is not null)
        {
            doc.Raise(new DocumentRenewalScheduledEvent(doc.Id, householdId, doc.Title, renewalDateUtc.Value, createdByUserId));
        }

        return doc;
    }
}
