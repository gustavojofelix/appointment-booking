using MediatR;

namespace Scheduling.Application.Admin.ListAppointments;

public sealed record ListAppointmentsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status
) : IRequest<PagedResult<AppointmentRowDto>>;

public sealed record AppointmentRowDto(
    Guid AppointmentId,
    Guid ProviderId,
    string ProviderName,
    Guid SlotId,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string Status,
    string CustomerName,
    string CustomerEmail,
    DateTime CreatedAtUtc
);

public sealed record PagedResult<T>(int Page, int PageSize, int TotalCount, IReadOnlyList<T> Items);
