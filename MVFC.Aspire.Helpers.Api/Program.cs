var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton(_ =>
{
    return new StorageClientBuilder
    {
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    }.Build();
});
builder.Services.AddSingleton(_ =>
{
    return new PublisherServiceApiClientBuilder
    {
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    }.Build();
});
builder.Services.AddSingleton<IStorageService, GoogleCloudStorageAdapter>();
builder.Services.AddSingleton<IMessagePublisher, GooglePubSubMessagePublisher>();

var app = builder.Build();
app.MapAllEndpoints();

await app.RunAsync();