namespace Scheduling.Application.Messaging;

public sealed record AppointmentBookedV1(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid AppointmentId,
    Guid ProviderId,
    Guid SlotId,
    string CustomerEmail
);
