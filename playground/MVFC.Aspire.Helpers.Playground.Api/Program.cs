var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(_ => {
    return new StorageClientBuilder {
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    }.Build();
});
builder.Services.AddSingleton(_ => {
    return new PublisherServiceApiClientBuilder {
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    }.Build();
});
builder.Services.AddSingleton<IStorageService, GoogleCloudStorageAdapter>();
builder.Services.AddSingleton<IMessagePublisher, GooglePubSubMessagePublisher>();
builder.Services.AddSingleton<SmtpClient>(_ => {
    var smtpUri = new Uri(builder.Configuration.GetConnectionString("mailpit")!);
    return new SmtpClient(smtpUri.Host, smtpUri.Port);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("redis")!));

builder.Services.AddSingleton<IConnection>(_ => {
    var factory = new ConnectionFactory {
        Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
    };
    return factory.CreateConnection();
});

var app = builder.Build();
app.MapAllEndpoints();

await app.RunAsync();