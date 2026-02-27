using Confluent.Kafka;

var topic = "test-events";

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    Acks = Acks.All,
    EnableIdempotence = true, // safer producer
};

using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

Console.WriteLine("Type a message and press Enter (empty line to quit).");

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line))
        break;

    try
    {
        var result = await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = "demo",   // optional, controls partitioning
            Value = line
        });

        Console.WriteLine($"✅ Produced to {result.TopicPartitionOffset}");
    }
    catch (ProduceException<string, string> ex)
    {
        Console.WriteLine($"❌ Produce failed: {ex.Error.Reason}");
    }
}

producer.Flush(TimeSpan.FromSeconds(5));