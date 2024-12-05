using Confluent.Kafka;

namespace AspireKafka.Subscriber;

public class Worker(IConsumer<Ignore, string> consumer, ILogger<Worker> logger) : BackgroundService
{
    private const string Topic = "messaging";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        long i = 0;
        return Task.Factory.StartNew(async () =>
        {
            consumer.Subscribe(Topic);
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? result = default;
                try
                {
                    result = consumer.Consume(stoppingToken);
                    if (result is not null)
                    {
                        logger.LogInformation("Consumed message [{Key}] = {Value}",
                            result.Message?.Key,
                            result.Message?.Value);
                    }
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    await Task.Delay(100);
                    continue;
                }

                i++;
                if (i % 1000 == 0)
                {
                    logger.LogInformation($"Received {i} messages. current offset is '{result!.Offset}'");
                }
            }
        }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        consumer.Unsubscribe();
        consumer.Dispose();
        await base.StopAsync(stoppingToken);
    }
}