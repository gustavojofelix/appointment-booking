using MediatR;
using Scheduling.Application.Slots.ListAvailableSlots;

namespace Scheduling.Api.Endpoints;

public static class SlotsEndpoints
{
  public static IEndpointRouteBuilder MapSlotsEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/providers/{providerId:guid}/slots")
        .WithTags("Slots");

    group.MapGet("", async (
        Guid providerId,
        DateTime fromUtc,
        DateTime toUtc,
        IMediator mediator,
        CancellationToken ct) =>
    {
      var result = await mediator.Send(new ListAvailableSlotsQuery(providerId, fromUtc, toUtc), ct);
      return Results.Ok(result);
    });

    return app;
  }
}
