var builder = DistributedApplication.CreateBuilder(args);

var kafka =  builder.AddKafka("kafka")
    .WithKafkaUI()
    .WithLifetime(ContainerLifetime.Session);

var subscriber = builder.AddProject<Projects.AspireKafka_Subscriber>("subscriber")
    .WithReference(kafka).WaitFor(kafka);

var publisher = builder.AddProject<Projects.AspireKafka_Publisher>("publisher")
    .WithReference(subscriber).WaitFor(subscriber)
    .WithReference(kafka).WaitFor(kafka);

builder.Build().Run();
