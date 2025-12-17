using MediatR;

namespace Scheduling.Application.Appointments.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid AppointmentId) : IRequest;
