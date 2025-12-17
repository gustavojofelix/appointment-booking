namespace Scheduling.Worker.Mongo;

public sealed class MongoOptions
{
  public string ConnectionString { get; set; } = "mongodb://localhost:27017";
  public string Database { get; set; } = "SchedulingAudit";
  public string Collection { get; set; } = "appointment_audit";
}
