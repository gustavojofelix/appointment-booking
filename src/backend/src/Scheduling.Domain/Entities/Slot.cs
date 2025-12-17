using Scheduling.Domain.Common;

namespace Scheduling.Domain.Entities;

public enum SlotStatus { Available = 0, Reserved = 1 }

public sealed class Slot
{
  public Guid Id { get; private set; } = Guid.NewGuid();
  public Guid ProviderId { get; private set; }
  public DateTime StartUtc { get; private set; }
  public DateTime EndUtc { get; private set; }
  public SlotStatus Status { get; private set; } = SlotStatus.Available;

  private Slot() { }

  public Slot(Guid providerId, DateTime startUtc, DateTime endUtc)
  {
    if (endUtc <= startUtc) throw new DomainException("Slot end must be after start.");
    ProviderId = providerId;
    StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
    EndUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
    Status = SlotStatus.Available;
  }
}
