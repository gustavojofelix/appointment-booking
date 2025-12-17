namespace Scheduling.Application.Messaging;

public interface IAppointmentEventsPublisher
{
  Task PublishBookedAsync(AppointmentBookedV1 evt, CancellationToken ct);
  Task PublishCancelledAsync(AppointmentCancelledV1 evt, CancellationToken ct);
  Task PublishRescheduledAsync(AppointmentRescheduledV1 evt, CancellationToken ct);
}
