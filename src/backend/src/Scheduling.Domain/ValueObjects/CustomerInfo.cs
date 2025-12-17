using Scheduling.Domain.Common;

namespace Scheduling.Domain.ValueObjects;

public sealed record CustomerInfo(string Name, string Email, string Phone)
{
  public static CustomerInfo Create(string name, string email, string phone)
  {
    if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Customer name is required.");
    if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Customer email is required.");
    if (string.IsNullOrWhiteSpace(phone)) throw new DomainException("Customer phone is required.");

    return new CustomerInfo(
        name.Trim(),
        email.Trim().ToLowerInvariant(),
        phone.Trim()
    );
  }
}
