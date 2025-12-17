using Scheduling.Domain.ValueObjects;

namespace Scheduling.Domain.Entities;

public enum AppointmentStatus
{
    Booked = 0,
    Cancelled = 1,
}

public sealed class Appointment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProviderId { get; private set; }
    public Guid SlotId { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Booked;

    public string CustomerName { get; private set; } = default!;
    public string CustomerEmail { get; private set; } = default!;
    public string CustomerPhone { get; private set; } = default!;
    public string? Reason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Appointment() { }

    public static Appointment Book(
        Guid providerId,
        Guid slotId,
        CustomerInfo customer,
        string? reason
    )
    {
        return new Appointment
        {
            ProviderId = providerId,
            SlotId = slotId,
            Status = AppointmentStatus.Booked,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            CustomerPhone = customer.Phone,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Cancel()
    {
        if (Status != AppointmentStatus.Booked)
            throw new Scheduling.Domain.Common.DomainException(
                "Only booked appointments can be cancelled."
            );

        Status = AppointmentStatus.Cancelled;
    }

    public void Reschedule(Guid newSlotId)
    {
        if (Status != AppointmentStatus.Booked)
            throw new Scheduling.Domain.Common.DomainException(
                "Only booked appointments can be rescheduled."
            );

        if (newSlotId == Guid.Empty)
            throw new Scheduling.Domain.Common.DomainException("New slot is required.");

        SlotId = newSlotId;
    }
}
