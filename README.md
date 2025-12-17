# Appointment Booking Demo (Angular + .NET 10 + Kafka + Mongo)

## Run dependencies

```powershell
docker compose -f .\deploy\docker\docker-compose.yml up -d
docker exec -it scheduling-redpanda rpk topic create scheduling.appointments --partitions 3 --replicas 1
```
