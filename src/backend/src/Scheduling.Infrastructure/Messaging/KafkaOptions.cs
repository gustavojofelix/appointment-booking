namespace Scheduling.Infrastructure.Messaging;

public sealed class KafkaOptions
{
  public string BootstrapServers { get; set; } = "localhost:9092";
  public string ClientId { get; set; } = "scheduling-api";
  public string AppointmentsTopic { get; set; } = "scheduling.appointments";
}
