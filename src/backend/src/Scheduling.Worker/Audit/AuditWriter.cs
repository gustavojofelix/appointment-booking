using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Scheduling.Application.Messaging;
using Scheduling.Worker.Mongo;

namespace Scheduling.Worker.Audit;

public sealed class AuditWriter
{
    private readonly IMongoCollection<AuditEventDocument> _collection;

    public AuditWriter(IOptions<MongoOptions> opt)
    {
        var o = opt.Value;
        var client = new MongoClient(o.ConnectionString);
        var db = client.GetDatabase(o.Database);
        _collection = db.GetCollection<AuditEventDocument>(o.Collection);

        // Ensure EventId is unique (idempotency)
        var keys = Builders<AuditEventDocument>.IndexKeys.Ascending(x => x.EventId);
        var model = new CreateIndexModel<AuditEventDocument>(
            keys,
            new CreateIndexOptions { Unique = true }
        );
        _collection.Indexes.CreateOne(model);
    }

    public async Task TryWriteAsync(
        Guid eventId,
        string eventType,
        Guid appointmentId,
        string rawJson,
        CancellationToken ct
    )
    {
        var doc = new AuditEventDocument
        {
            EventId = eventId,
            Type = eventType,
            AppointmentId = appointmentId,
            ReceivedAtUtc = DateTime.UtcNow,
            PayloadJson = rawJson,
        };

        try
        {
            await _collection.InsertOneAsync(doc, cancellationToken: ct);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // ignore duplicates
        }
    }
}
