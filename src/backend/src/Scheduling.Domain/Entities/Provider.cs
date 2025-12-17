namespace Scheduling.Domain.Entities;

public sealed class Provider
{
  public Guid Id { get; private set; } = Guid.NewGuid();
  public string Name { get; private set; } = default!;

  private Provider() { }

  public Provider(string name)
  {
    Name = name.Trim();
  }
}
