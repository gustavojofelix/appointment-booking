using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Slots.ListAvailableSlots;

public sealed class ListAvailableSlotsHandler : IRequestHandler<ListAvailableSlotsQuery, IReadOnlyList<SlotDto>>
{
  private readonly ISchedulingDb _db;
  public ListAvailableSlotsHandler(ISchedulingDb db) => _db = db;

  public async Task<IReadOnlyList<SlotDto>> Handle(ListAvailableSlotsQuery request, CancellationToken ct)
  {
    var from = DateTime.SpecifyKind(request.FromUtc, DateTimeKind.Utc);
    var to = DateTime.SpecifyKind(request.ToUtc, DateTimeKind.Utc);

    return await _db.Slots
        .Where(s => s.ProviderId == request.ProviderId
                    && s.Status == SlotStatus.Available
                    && s.StartUtc >= from
                    && s.StartUtc < to)
        .OrderBy(s => s.StartUtc)
        .Select(s => new SlotDto(s.Id, s.StartUtc, s.EndUtc))
        .ToListAsync(ct);
  }
}
