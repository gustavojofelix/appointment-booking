namespace Scheduling.Application.Messaging;

public sealed record AppointmentRescheduledV1(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid AppointmentId,
    Guid ProviderId,
    Guid OldSlotId,
    Guid NewSlotId
);
