using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Scheduling.Worker.Audit;
using Scheduling.Worker.Kafka;
using Scheduling.Worker.Mongo;

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));


var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerKafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<AuditWriter>();
builder.Services.AddHostedService<AppointmentEventsConsumer>();

var host = builder.Build();
await host.RunAsync();
