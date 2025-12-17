using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Errors;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Slots.GenerateSlots;

public sealed class GenerateSlotsHandler
    : IRequestHandler<GenerateSlotsCommand, GenerateSlotsResult>
{
    private readonly ISchedulingDb _db;

    public GenerateSlotsHandler(ISchedulingDb db) => _db = db;

    public async Task<GenerateSlotsResult> Handle(
        GenerateSlotsCommand request,
        CancellationToken ct
    )
    {
        if (request.Days is < 1 or > 60)
            throw new ArgumentOutOfRangeException(
                nameof(request.Days),
                "Days must be between 1 and 60."
            );
        if (request.SlotMinutes is < 10 or > 120)
            throw new ArgumentOutOfRangeException(
                nameof(request.SlotMinutes),
                "SlotMinutes must be between 10 and 120."
            );
        if (request.StartHourLocal is < 0 or > 23)
            throw new ArgumentOutOfRangeException(nameof(request.StartHourLocal));
        if (request.EndHourLocal is < 0 or > 24)
            throw new ArgumentOutOfRangeException(nameof(request.EndHourLocal));
        if (request.EndHourLocal <= request.StartHourLocal)
            throw new ArgumentOutOfRangeException(
                nameof(request.EndHourLocal),
                "EndHourLocal must be after StartHourLocal."
            );

        var providerExists = await _db.Providers.AnyAsync(p => p.Id == request.ProviderId, ct);
        if (!providerExists)
            throw new NotFoundException("Provider not found.");

        var todayUtc = DateTime.UtcNow.Date;
        var fromUtc = todayUtc;
        var toUtc = todayUtc.AddDays(request.Days);

        // Avoid duplicates: only create slots that don't already exist for that provider+start time.
        var existingStarts = await _db
            .Slots.Where(s =>
                s.ProviderId == request.ProviderId && s.StartUtc >= fromUtc && s.StartUtc < toUtc
            )
            .Select(s => s.StartUtc)
            .ToListAsync(ct);

        var existing = existingStarts.ToHashSet();

        var slotsToCreate = new List<Slot>(capacity: request.Days * 32);

        for (var d = 0; d < request.Days; d++)
        {
            var day = todayUtc.AddDays(d);

            // Using "local hours" but stored as UTC for simplicity in demo.
            // Later we can introduce a timezone strategy.
            var start = new DateTime(
                day.Year,
                day.Month,
                day.Day,
                request.StartHourLocal,
                0,
                0,
                DateTimeKind.Utc
            );
            var end = new DateTime(
                day.Year,
                day.Month,
                day.Day,
                request.EndHourLocal,
                0,
                0,
                DateTimeKind.Utc
            );

            for (
                var t = start;
                t.AddMinutes(request.SlotMinutes) <= end;
                t = t.AddMinutes(request.SlotMinutes)
            )
            {
                if (existing.Contains(t))
                    continue;
                slotsToCreate.Add(
                    new Slot(request.ProviderId, t, t.AddMinutes(request.SlotMinutes))
                );
            }
        }

        if (slotsToCreate.Count == 0)
            return new GenerateSlotsResult(0);

        await _db.AddSlotsAsync(slotsToCreate, ct);
        await _db.SaveChangesAsync(ct);

        return new GenerateSlotsResult(slotsToCreate.Count);
    }
}
