using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDbContext : DbContext, ISchedulingDb
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options) { }

    public DbSet<Provider> ProvidersSet => Set<Provider>();
    public DbSet<Slot> SlotsSet => Set<Slot>();
    public DbSet<Appointment> AppointmentsSet => Set<Appointment>();

    public IQueryable<Provider> Providers => ProvidersSet.AsNoTracking();
    public IQueryable<Slot> Slots => SlotsSet.AsNoTracking();
    public IQueryable<Appointment> Appointments => AppointmentsSet.AsNoTracking();

    public Task AddSlotsAsync(IEnumerable<Slot> slots, CancellationToken ct) =>
        SlotsSet.AddRangeAsync(slots, ct);

    public Task AddAppointmentAsync(Appointment appointment, CancellationToken ct) =>
        AppointmentsSet.AddAsync(appointment, ct).AsTask();

    public async Task<int> TryReserveSlotAsync(Guid slotId, CancellationToken ct)
    {
        // Requires EF Core ExecuteUpdateAsync support (EF7+). If your EF version differs,
        // we can swap to raw SQL easily.
        return await SlotsSet
            .Where(s => s.Id == slotId && s.Status == SlotStatus.Available)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Status, SlotStatus.Reserved), ct);
    }

    public Task<Appointment?> GetAppointmentForUpdateAsync(
        Guid appointmentId,
        CancellationToken ct
    ) => AppointmentsSet.FirstOrDefaultAsync(a => a.Id == appointmentId, ct);

    public async Task<int> TryReleaseSlotAsync(Guid slotId, CancellationToken ct)
    {
        return await SlotsSet
            .Where(s => s.Id == slotId && s.Status == SlotStatus.Reserved)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Status, SlotStatus.Available), ct);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken ct
    )
    {
        await using var tx = await Database.BeginTransactionAsync(ct);
        try
        {
            await action(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Provider>(b =>
        {
            b.ToTable("Providers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Slot>(b =>
        {
            b.ToTable("Slots");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).IsRequired();
            b.HasIndex(x => new { x.ProviderId, x.StartUtc }).IsUnique(); // prevents duplicate slot generation
            b.HasIndex(x => new
            {
                x.ProviderId,
                x.StartUtc,
                x.Status,
            });
        });

        modelBuilder.Entity<Appointment>(b =>
        {
            b.ToTable("Appointments");
            b.HasKey(x => x.Id);
            b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            b.Property(x => x.CustomerEmail).HasMaxLength(320).IsRequired();
            b.Property(x => x.CustomerPhone).HasMaxLength(50).IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.HasIndex(x => x.SlotId).IsUnique(); // hard safety: only one appointment per slot
        });
    }
}
