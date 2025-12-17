using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Errors;
using Scheduling.Application.Messaging;

namespace Scheduling.Application.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentHandler : IRequestHandler<RescheduleAppointmentCommand>
{
  private readonly ISchedulingDb _db;
  private readonly IAppointmentEventsPublisher _publisher;

  public RescheduleAppointmentHandler(ISchedulingDb db, IAppointmentEventsPublisher publisher)
  {
    _db = db;
    _publisher = publisher;
  }

  public async Task Handle(RescheduleAppointmentCommand request, CancellationToken ct)
  {
    var appointmentId = request.AppointmentId;
    var newSlotId = request.NewSlotId;

    Guid providerId = Guid.Empty;
    Guid oldSlotId = Guid.Empty;

    await _db.ExecuteInTransactionAsync(async token =>
    {
      var appt = await _db.GetAppointmentForUpdateAsync(appointmentId, token);
      if (appt is null) throw new NotFoundException("Appointment not found.");

      providerId = appt.ProviderId;
      oldSlotId = appt.SlotId;

      // Ensure the new slot is for the same provider (simple rule for demo)
      var newSlotBelongsToProvider = await _db.Slots.AnyAsync(s => s.Id == newSlotId && s.ProviderId == appt.ProviderId, token);
      if (!newSlotBelongsToProvider) throw new NotFoundException("New slot not found for provider.");

      var reserved = await _db.TryReserveSlotAsync(newSlotId, token);
      if (reserved == 0) throw new ConflictException("New slot already reserved.");

      appt.Reschedule(newSlotId);

      await _db.TryReleaseSlotAsync(oldSlotId, token);

      await _db.SaveChangesAsync(token);
    }, ct);

    var evt = new AppointmentRescheduledV1(
        EventId: Guid.NewGuid(),
        OccurredAtUtc: DateTime.UtcNow,
        AppointmentId: appointmentId,
        ProviderId: providerId,
        OldSlotId: oldSlotId,
        NewSlotId: newSlotId
    );

    await _publisher.PublishRescheduledAsync(evt, ct);
  }
}
