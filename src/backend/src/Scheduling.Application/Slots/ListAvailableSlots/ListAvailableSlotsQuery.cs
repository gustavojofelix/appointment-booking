using MediatR;

namespace Scheduling.Application.Slots.ListAvailableSlots;

public sealed record ListAvailableSlotsQuery(Guid ProviderId, DateTime FromUtc, DateTime ToUtc)
    : IRequest<IReadOnlyList<SlotDto>>;

public sealed record SlotDto(Guid Id, DateTime StartUtc, DateTime EndUtc);
