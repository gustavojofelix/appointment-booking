namespace Scheduling.Api.Endpoints;

public static class EndpointExtensions
{
  public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapProvidersEndpoints();
    app.MapSlotsEndpoints();
    app.MapAppointmentsEndpoints();
    app.MapAdminEndpoints();
    return app;
  }
}
