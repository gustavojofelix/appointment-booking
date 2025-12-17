using MediatR;

namespace Scheduling.Application.Appointments.BookAppointment;

public sealed record BookAppointmentCommand(
    Guid ProviderId,
    Guid SlotId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string? Reason
) : IRequest<BookAppointmentResult>;

public sealed record BookAppointmentResult(Guid AppointmentId);
