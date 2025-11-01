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

var app = builder.Build();
app.MapAllEndpoints();

await app.RunAsync();