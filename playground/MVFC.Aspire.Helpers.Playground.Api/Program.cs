var builder = WebApplication.CreateBuilder(args);

var storageClient = await InstanceHelpers.CreateStorageClientAsync();
var pubSubClient = await InstanceHelpers.CreatePubSubClientAsync();
var redis = await builder.CreateRedisAsync();
var rabbit = await builder.CreateRabbitAsync();

builder.Services.AddSingleton(pubSubClient);
builder.Services.AddSingleton(storageClient);
builder.Services.AddSingleton(builder.CreateSmtp());
builder.Services.AddSingleton(redis);
builder.Services.AddSingleton(rabbit);
builder.Services.AddSingleton<IStorageService, GoogleCloudStorageAdapter>();
builder.Services.AddSingleton<IMessagePublisher, GooglePubSubMessagePublisher>();

builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer((document, context, cancellationToken) => {
        document.Servers = [];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options.Servers = [];
    });
}

app.MapAllEndpoints();

await app.RunAsync();