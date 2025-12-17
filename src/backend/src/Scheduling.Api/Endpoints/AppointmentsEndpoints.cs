using MediatR;
using Scheduling.Application.Appointments.BookAppointment;
using Scheduling.Application.Appointments.CancelAppointment;
using Scheduling.Application.Appointments.RescheduleAppointment;

namespace Scheduling.Api.Endpoints;

public static class AppointmentsEndpoints
{
  public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/appointments")
        .WithTags("Appointments");

    group.MapPost("", async (BookAppointmentCommand cmd, IMediator mediator, CancellationToken ct) =>
    {
      var result = await mediator.Send(cmd, ct);
      return Results.Created($"/api/appointments/{result.AppointmentId}", result);
    });

    group.MapPut("{appointmentId:guid}/cancel", async (Guid appointmentId, IMediator mediator, CancellationToken ct) =>
    {
      await mediator.Send(new CancelAppointmentCommand(appointmentId), ct);
      return Results.NoContent();
    });

    group.MapPut("{appointmentId:guid}/reschedule", async (
        Guid appointmentId,
        Guid newSlotId,
        IMediator mediator,
        CancellationToken ct) =>
    {
      await mediator.Send(new RescheduleAppointmentCommand(appointmentId, newSlotId), ct);
      return Results.NoContent();
    });

    return app;
  }
}
