using MediatR;

namespace Scheduling.Application.Providers.ListProviders;

public sealed record ListProvidersQuery() : IRequest<IReadOnlyList<ProviderDto>>;
public sealed record ProviderDto(Guid Id, string Name);
