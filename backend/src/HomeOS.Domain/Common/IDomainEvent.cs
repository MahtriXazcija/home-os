namespace HomeOS.Domain.Common;

/// <summary>
/// Marker for events raised by aggregates. This is the seam every future
/// app (built-in or third-party) plugs into instead of calling other
/// modules directly — see docs/app-sdk.md.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
