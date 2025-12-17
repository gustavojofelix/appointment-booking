namespace Scheduling.Application.Messaging;

public sealed record AppointmentCancelledV1(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid AppointmentId,
    Guid ProviderId,
    Guid SlotId
);
