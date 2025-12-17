using MediatR;
using Scheduling.Application.Admin.ListAppointments;
using Scheduling.Application.Slots.GenerateSlots;

namespace Scheduling.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");

        group.MapPost(
            "providers/{providerId:guid}/slots/generate",
            async (
                Guid providerId,
                int days,
                int slotMinutes,
                int startHourLocal,
                int endHourLocal,
                IMediator mediator,
                CancellationToken ct
            ) =>
            {
                var result = await mediator.Send(
                    new GenerateSlotsCommand(
                        providerId,
                        days,
                        slotMinutes,
                        startHourLocal,
                        endHourLocal
                    ),
                    ct
                );

                return Results.Ok(result);
            }
        );

        group.MapGet(
            "appointments",
            async (
                int page,
                int pageSize,
                string? search,
                string? status,
                IMediator mediator,
                CancellationToken ct
            ) =>
            {
                var result = await mediator.Send(
                    new ListAppointmentsQuery(page, pageSize, search, status),
                    ct
                );
                return Results.Ok(result);
            }
        );

        return app;
    }
}
