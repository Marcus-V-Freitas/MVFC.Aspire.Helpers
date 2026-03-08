var builder = WebApplication.CreateBuilder(args);

var storageClient = await InstanceHelpers.CreateStorageClientAsync().ConfigureAwait(false);
var pubSubClient = await InstanceHelpers.CreatePubSubClientAsync().ConfigureAwait(false);
var redis = await builder.CreateRedisAsync().ConfigureAwait(false);
var rabbit = await builder.CreateRabbitAsync().ConfigureAwait(false);

builder.Services.AddKeycloak(builder.Configuration);
builder.Services.AddGotenberg();
builder.Services.AddSingleton(pubSubClient);
builder.Services.AddSingleton(storageClient);
builder.Services.AddSingleton(builder.CreateSmtp());
builder.Services.AddSingleton(redis);
builder.Services.AddSingleton(rabbit);
builder.Services.AddSingleton<IStorageService, GoogleCloudStorageAdapter>();
builder.Services.AddSingleton<IMessagePublisher, GooglePubSubMessagePublisher>();
builder.Services.AddScoped<IGotenbergService, GotenbergService>();
builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformer>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
{
    document.Servers = [];
    return Task.CompletedTask;
}));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Servers = []);
}

app.MapAllEndpoints();

await app.RunAsync().ConfigureAwait(false);
