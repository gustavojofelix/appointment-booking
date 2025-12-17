using MediatR;

namespace Scheduling.Application.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(Guid AppointmentId, Guid NewSlotId) : IRequest;
