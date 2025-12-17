using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
  public SchedulingDbContext CreateDbContext(string[] args)
  {
    var cs = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer");
    if (string.IsNullOrWhiteSpace(cs))
    {
      throw new InvalidOperationException(
          "Missing ConnectionStrings__SqlServer environment variable. " +
          "Set it before running dotnet ef."
      );
    }

    var options = new DbContextOptionsBuilder<SchedulingDbContext>()
        .UseSqlServer(cs)
        .Options;

    return new SchedulingDbContext(options);
  }
}
