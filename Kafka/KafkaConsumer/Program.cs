using Confluent.Kafka;

var topic = "test-events";

var consumerConfig = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "demo-consumer-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false, // commit after processing
};

using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
    .SetErrorHandler((_, e) => Console.WriteLine($"Kafka error: {e.Reason}"))
    .Build();

consumer.Subscribe(topic);

Console.WriteLine("📥 Consuming. Press Ctrl+C to stop.");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    while (!cts.IsCancellationRequested)
    {
        var cr = consumer.Consume(cts.Token);

        Console.WriteLine($"[{cr.TopicPartitionOffset}] key={cr.Message.Key}, value={cr.Message.Value}");

        // Mark message processed
        consumer.Commit(cr);
    }
}
catch (OperationCanceledException)
{
    // graceful shutdown
}
finally
{
    consumer.Close();
}