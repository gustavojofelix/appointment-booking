using MediatR;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Errors;
using Scheduling.Application.Messaging;

namespace Scheduling.Application.Appointments.CancelAppointment;

public sealed class CancelAppointmentHandler : IRequestHandler<CancelAppointmentCommand>
{
    private readonly ISchedulingDb _db;
    private readonly IAppointmentEventsPublisher _publisher;

    public CancelAppointmentHandler(ISchedulingDb db, IAppointmentEventsPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        Guid appointmentId = request.AppointmentId;
        Guid providerId = Guid.Empty;
        Guid slotId = Guid.Empty;

        await _db.ExecuteInTransactionAsync(
            async token =>
            {
                var appt = await _db.GetAppointmentForUpdateAsync(appointmentId, token);
                if (appt is null)
                    throw new NotFoundException("Appointment not found.");

                providerId = appt.ProviderId;
                slotId = appt.SlotId;

                appt.Cancel();

                // release the slot (idempotent-ish: 0 rows means already available)
                await _db.TryReleaseSlotAsync(slotId, token);

                await _db.SaveChangesAsync(token);
            },
            ct
        );

        var evt = new AppointmentCancelledV1(
            EventId: Guid.NewGuid(),
            OccurredAtUtc: DateTime.UtcNow,
            AppointmentId: appointmentId,
            ProviderId: providerId,
            SlotId: slotId
        );

        await _publisher.PublishCancelledAsync(evt, ct);
    }
}
