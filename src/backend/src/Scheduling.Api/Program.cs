using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduling.Api.Endpoints;
using Scheduling.Application;
using Scheduling.Application.Admin.ListAppointments;
using Scheduling.Application.Appointments.BookAppointment;
using Scheduling.Application.Appointments.CancelAppointment;
using Scheduling.Application.Appointments.RescheduleAppointment;
using Scheduling.Application.Errors;
using Scheduling.Application.Providers.ListProviders;
using Scheduling.Application.Slots.GenerateSlots;
using Scheduling.Application.Slots.ListAvailableSlots;
using Scheduling.Infrastructure;
using Scheduling.Infrastructure.Messaging;
using Scheduling.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// global exception mapping
app.Use(
    async (ctx, next) =>
    {
        try
        {
            await next();
        }
        catch (NotFoundException ex)
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404,
                }
            );
        }
        catch (ConflictException ex)
        {
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = 409,
                }
            );
        }
    }
);

app.MapGet("/health", () => Results.Ok("OK"));

app.MapApiEndpoints();

// Auto-migrate + seed providers for demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.ProvidersSet.AnyAsync())
    {
        db.ProvidersSet.AddRange(
            new Scheduling.Domain.Entities.Provider("Dr. A. Ndlovu"),
            new Scheduling.Domain.Entities.Provider("Dr. M. Patel"),
            new Scheduling.Domain.Entities.Provider("Dr. S. Moyo")
        );
        await db.SaveChangesAsync();
    }
}

app.Run();
