using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Scheduling.Application.Messaging;

namespace Scheduling.Infrastructure.Messaging;

public sealed class KafkaAppointmentEventsPublisher : IAppointmentEventsPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _opt;

    public KafkaAppointmentEventsPublisher(
        IProducer<string, string> producer,
        IOptions<KafkaOptions> opt
    )
    {
        _producer = producer;
        _opt = opt.Value;
    }

    public Task PublishBookedAsync(AppointmentBookedV1 evt, CancellationToken ct) =>
        PublishAsync(nameof(AppointmentBookedV1), evt.AppointmentId.ToString(), evt);

    public Task PublishCancelledAsync(AppointmentCancelledV1 evt, CancellationToken ct) =>
        PublishAsync(nameof(AppointmentCancelledV1), evt.AppointmentId.ToString(), evt);

    public Task PublishRescheduledAsync(AppointmentRescheduledV1 evt, CancellationToken ct) =>
        PublishAsync(nameof(AppointmentRescheduledV1), evt.AppointmentId.ToString(), evt);

    private Task PublishAsync<T>(string eventType, string key, T payload)
    {
        var json = JsonSerializer.Serialize(payload);

        var msg = new Message<string, string>
        {
            Key = key,
            Value = json,
            Headers = new Headers
            {
                new Header("eventType", Encoding.UTF8.GetBytes(eventType)),
                new Header("eventVersion", Encoding.UTF8.GetBytes("1")),
            },
        };

        return _producer.ProduceAsync(_opt.AppointmentsTopic, msg);
    }
}
