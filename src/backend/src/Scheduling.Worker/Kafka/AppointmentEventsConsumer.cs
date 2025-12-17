using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduling.Application.Messaging;
using Scheduling.Worker.Audit;

namespace Scheduling.Worker.Kafka;

public sealed class AppointmentEventsConsumer : BackgroundService
{
    private readonly WorkerKafkaOptions _opt;
    private readonly AuditWriter _audit;
    private readonly ILogger<AppointmentEventsConsumer> _logger;

    public AppointmentEventsConsumer(
        IOptions<WorkerKafkaOptions> opt,
        AuditWriter audit,
        ILogger<AppointmentEventsConsumer> logger
    )
    {
        _opt = opt.Value;
        _audit = audit;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = _opt.BootstrapServers,
            GroupId = _opt.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true, // demo-friendly; in production commit after successful processing
        };

        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();
        consumer.Subscribe(_opt.AppointmentsTopic);

        _logger.LogInformation(
            "Worker consuming topic {Topic} (group {Group})",
            _opt.AppointmentsTopic,
            _opt.GroupId
        );

        var gate = new SemaphoreSlim(6); // bounded parallelism
        var inFlight = new List<Task>(64);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? cr = null;

                try
                {
                    cr = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (cr?.Message?.Value is null)
                    continue;

                await gate.WaitAsync(stoppingToken);

                var task = Task.Run(
                    async () =>
                    {
                        try
                        {
                            var jsonOptions = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                            };

                            var raw = cr.Message.Value;

                            // Determine event type from headers (fallback to legacy booked-only format)
                            var typeHeader = cr.Message.Headers?.GetLastBytes("eventType");
                            var eventType = typeHeader is null
                                ? nameof(AppointmentBookedV1)
                                : Encoding.UTF8.GetString(typeHeader);

                            switch (eventType)
                            {
                                case nameof(AppointmentBookedV1):
                                {
                                    var evt = JsonSerializer.Deserialize<AppointmentBookedV1>(
                                        raw,
                                        jsonOptions
                                    );
                                    if (evt is null)
                                    {
                                        _logger.LogWarning(
                                            "Failed to deserialize AppointmentBookedV1"
                                        );
                                        return;
                                    }
                                    await _audit.TryWriteAsync(
                                        evt.EventId,
                                        eventType,
                                        evt.AppointmentId,
                                        raw,
                                        stoppingToken
                                    );
                                    break;
                                }

                                case nameof(AppointmentCancelledV1):
                                {
                                    var evt = JsonSerializer.Deserialize<AppointmentCancelledV1>(
                                        raw,
                                        jsonOptions
                                    );
                                    if (evt is null)
                                    {
                                        _logger.LogWarning(
                                            "Failed to deserialize AppointmentCancelledV1"
                                        );
                                        return;
                                    }
                                    await _audit.TryWriteAsync(
                                        evt.EventId,
                                        eventType,
                                        evt.AppointmentId,
                                        raw,
                                        stoppingToken
                                    );
                                    break;
                                }

                                case nameof(AppointmentRescheduledV1):
                                {
                                    var evt = JsonSerializer.Deserialize<AppointmentRescheduledV1>(
                                        raw,
                                        jsonOptions
                                    );
                                    if (evt is null)
                                    {
                                        _logger.LogWarning(
                                            "Failed to deserialize AppointmentRescheduledV1"
                                        );
                                        return;
                                    }
                                    await _audit.TryWriteAsync(
                                        evt.EventId,
                                        eventType,
                                        evt.AppointmentId,
                                        raw,
                                        stoppingToken
                                    );
                                    break;
                                }

                                default:
                                    _logger.LogWarning(
                                        "Unknown event type {EventType}. Raw message ignored.",
                                        eventType
                                    );
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message");
                        }
                        finally
                        {
                            gate.Release();
                        }
                    },
                    stoppingToken
                );

                inFlight.Add(task);

                // cleanup completed tasks
                if (inFlight.Count > 200)
                {
                    inFlight.RemoveAll(t => t.IsCompleted);
                }
            }
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch
            { /* ignore */
            }
            await Task.WhenAll(inFlight);
        }
    }
}
