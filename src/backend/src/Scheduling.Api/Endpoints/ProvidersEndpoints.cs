using MediatR;
using Scheduling.Application.Providers.ListProviders;

namespace Scheduling.Api.Endpoints;

public static class ProvidersEndpoints
{
  public static IEndpointRouteBuilder MapProvidersEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/providers")
        .WithTags("Providers");

    group.MapGet("", async (IMediator mediator, CancellationToken ct) =>
    {
      var result = await mediator.Send(new ListProvidersQuery(), ct);
      return Results.Ok(result);
    });

    return app;
  }
}
