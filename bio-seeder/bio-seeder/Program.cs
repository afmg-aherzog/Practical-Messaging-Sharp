﻿// See https://aka.ms/new-console-template for more information

using bio_seeder;
using Confluent.Kafka;

const string topic = "biography";

var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
};

using var producer = new ProducerBuilder<string, string>(config).Build();
var biographies = new BiographySeeder();
foreach(var bio in biographies)
{
    producer.Produce(topic, new Message<string, string> { Key = bio.name, Value = bio.biography },
        (deliveryReport) =>
        {
            if (deliveryReport.Error.Code != ErrorCode.NoError) {
                Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
            }
            else {
                Console.WriteLine($"Produced event to topic {topic}: key = {bio.name} value = {bio.biography}");
            }
        }
    );
}

int outstandingCount = producer.Flush(TimeSpan.FromSeconds(10));
while (outstandingCount != 0)
{
    //loop until we flush
    Console.WriteLine($"Still waiting on {outstandingCount} outstanding messages to flush");
    outstandingCount = producer.Flush(TimeSpan.FromSeconds(10));
}