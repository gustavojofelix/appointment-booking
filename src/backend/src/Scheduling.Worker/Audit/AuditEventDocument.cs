using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scheduling.Worker.Audit;

public sealed class AuditEventDocument
{
  [BsonId]
  public ObjectId Id { get; set; }

  public Guid EventId { get; set; }
  public string Type { get; set; } = default!;
  public Guid AppointmentId { get; set; }
  public DateTime ReceivedAtUtc { get; set; }
  public string PayloadJson { get; set; } = default!;
}
