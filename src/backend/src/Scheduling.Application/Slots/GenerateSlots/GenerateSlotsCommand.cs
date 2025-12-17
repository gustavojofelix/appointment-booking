using MediatR;

namespace Scheduling.Application.Slots.GenerateSlots;

public sealed record GenerateSlotsCommand(
    Guid ProviderId,
    int Days,
    int SlotMinutes,
    int StartHourLocal,
    int EndHourLocal
) : IRequest<GenerateSlotsResult>;

public sealed record GenerateSlotsResult(int Created);
