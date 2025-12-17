using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Messaging;
using Scheduling.Infrastructure.Messaging;
using Scheduling.Infrastructure.Persistence;

namespace Scheduling.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
  {
    var cs = config.GetConnectionString("SqlServer")
             ?? throw new InvalidOperationException("Missing connection string: SqlServer");

    services.AddDbContext<SchedulingDbContext>(opt => opt.UseSqlServer(cs));

    services.AddScoped<ISchedulingDb>(sp => sp.GetRequiredService<SchedulingDbContext>());

    //services.Configure<KafkaOptions>(config.GetSection("Kafka"));

    services.AddSingleton<IProducer<string, string>>(sp =>
    {
      var opt = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;

      var producerConfig = new ProducerConfig
      {
        BootstrapServers = opt.BootstrapServers,
        ClientId = opt.ClientId,
        Acks = Acks.All
      };

      return new ProducerBuilder<string, string>(producerConfig).Build();
    });

    services.AddSingleton<IAppointmentEventsPublisher, KafkaAppointmentEventsPublisher>();


    return services;
  }
}
