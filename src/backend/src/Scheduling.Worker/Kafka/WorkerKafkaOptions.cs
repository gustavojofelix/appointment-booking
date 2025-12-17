namespace Scheduling.Worker.Kafka;

public sealed class WorkerKafkaOptions
{
  public string BootstrapServers { get; set; } = "localhost:9092";
  public string GroupId { get; set; } = "scheduling-worker";
  public string AppointmentsTopic { get; set; } = "scheduling.appointments";
}
