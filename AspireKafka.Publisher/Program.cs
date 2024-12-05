using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.AddKafkaProducer<string, string>("kafka");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/send", async (
    IProducer<string, string> producer, 
    string key,
    string value,
    CancellationToken stoppingToken) =>
{
    var message = new Message<string, string>()
    {
        Key = key,
        Value = value
    };

    var result = await producer.ProduceAsync("messaging", message, stoppingToken);

    return Results.Ok(new { partition = result.Partition.Value, offset = result.Offset.Value });
});

app.Run();
