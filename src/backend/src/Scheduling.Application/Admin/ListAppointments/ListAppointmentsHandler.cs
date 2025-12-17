using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Admin.ListAppointments;

public sealed class ListAppointmentsHandler : IRequestHandler<ListAppointmentsQuery, PagedResult<AppointmentRowDto>>
{
  private readonly ISchedulingDb _db;
  public ListAppointmentsHandler(ISchedulingDb db) => _db = db;

  public async Task<PagedResult<AppointmentRowDto>> Handle(ListAppointmentsQuery request, CancellationToken ct)
  {
    var page = request.Page <= 0 ? 1 : request.Page;
    var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

    var search = request.Search?.Trim();
    var status = request.Status?.Trim();

    var q =
        from a in _db.Appointments
        join s in _db.Slots on a.SlotId equals s.Id
        join p in _db.Providers on a.ProviderId equals p.Id
        select new { a, s, p };

    if (!string.IsNullOrWhiteSpace(search))
    {
      q = q.Where(x =>
          x.a.CustomerName.Contains(search!) ||
          x.a.CustomerEmail.Contains(search!));
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
      // expected: "Booked" or "Cancelled" (case-insensitive)
      if (Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out var parsed))
        q = q.Where(x => x.a.Status == parsed);
    }

    var total = await q.CountAsync(ct);

    var items = await q
        .OrderByDescending(x => x.s.StartUtc)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new AppointmentRowDto(
            x.a.Id,
            x.a.ProviderId,
            x.p.Name,
            x.a.SlotId,
            x.s.StartUtc,
            x.s.EndUtc,
            x.a.Status.ToString(),
            x.a.CustomerName,
            x.a.CustomerEmail,
            x.a.CreatedAtUtc
        ))
        .ToListAsync(ct);

    return new PagedResult<AppointmentRowDto>(page, pageSize, total, items);
  }
}
