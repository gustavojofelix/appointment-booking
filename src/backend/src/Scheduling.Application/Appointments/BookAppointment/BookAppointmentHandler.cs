using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Errors;
using Scheduling.Application.Messaging;
using Scheduling.Domain.Entities;
using Scheduling.Domain.ValueObjects;

namespace Scheduling.Application.Appointments.BookAppointment;

public sealed class BookAppointmentHandler
    : IRequestHandler<BookAppointmentCommand, BookAppointmentResult>
{
    private readonly ISchedulingDb _db;
    private readonly IAppointmentEventsPublisher _publisher;

    public BookAppointmentHandler(ISchedulingDb db, IAppointmentEventsPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<BookAppointmentResult> Handle(
        BookAppointmentCommand request,
        CancellationToken ct
    )
    {
        var providerExists = await _db.Providers.AnyAsync(p => p.Id == request.ProviderId, ct);
        if (!providerExists)
            throw new NotFoundException("Provider not found.");

        var slotExistsForProvider = await _db.Slots.AnyAsync(
            s => s.Id == request.SlotId && s.ProviderId == request.ProviderId,
            ct
        );
        if (!slotExistsForProvider)
            throw new NotFoundException("Slot not found for provider.");

        // Atomic reserve to prevent double booking.
        var reserved = await _db.TryReserveSlotAsync(request.SlotId, ct);
        if (reserved == 0)
            throw new ConflictException("Slot already reserved.");

        var customer = CustomerInfo.Create(
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone
        );
        var appointment = Appointment.Book(
            request.ProviderId,
            request.SlotId,
            customer,
            request.Reason
        );

        await _db.AddAppointmentAsync(appointment, ct);
        await _db.SaveChangesAsync(ct);

        // Publish event (Day 2)
        var evt = new AppointmentBookedV1(
            EventId: Guid.NewGuid(),
            OccurredAtUtc: DateTime.UtcNow,
            AppointmentId: appointment.Id,
            ProviderId: appointment.ProviderId,
            SlotId: appointment.SlotId,
            CustomerEmail: appointment.CustomerEmail
        );

        await _publisher.PublishBookedAsync(evt, ct);

        return new BookAppointmentResult(appointment.Id);
    }
}
