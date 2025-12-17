using Scheduling.Domain.Entities;

namespace Scheduling.Application.Abstractions;

public interface ISchedulingDb
{
  IQueryable<Provider> Providers { get; }
  IQueryable<Slot> Slots { get; }
  IQueryable<Appointment> Appointments { get; }

  Task AddSlotsAsync(IEnumerable<Slot> slots, CancellationToken ct);
  Task AddAppointmentAsync(Appointment appointment, CancellationToken ct);

  Task<Appointment?> GetAppointmentForUpdateAsync(Guid appointmentId, CancellationToken ct);
  Task<int> TryReleaseSlotAsync(Guid slotId, CancellationToken ct);

  Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct);


  /// <summary>
  /// Atomic reservation. Returns affected rows (1 = reserved, 0 = already reserved / missing).
  /// </summary>
  Task<int> TryReserveSlotAsync(Guid slotId, CancellationToken ct);

  Task<int> SaveChangesAsync(CancellationToken ct);
}
