using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;

namespace Scheduling.Application.Providers.ListProviders;

public sealed class ListProvidersHandler : IRequestHandler<ListProvidersQuery, IReadOnlyList<ProviderDto>>
{
  private readonly ISchedulingDb _db;
  public ListProvidersHandler(ISchedulingDb db) => _db = db;

  public async Task<IReadOnlyList<ProviderDto>> Handle(ListProvidersQuery request, CancellationToken ct)
  {
    return await _db.Providers
        .OrderBy(p => p.Name)
        .Select(p => new ProviderDto(p.Id, p.Name))
        .ToListAsync(ct);
  }
}
