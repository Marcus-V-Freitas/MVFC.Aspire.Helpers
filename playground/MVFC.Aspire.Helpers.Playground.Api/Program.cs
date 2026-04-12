var builder = WebApplication.CreateBuilder(args);

var firestoreDbs = await builder.CreateFirestoreDbsAsync().ConfigureAwait(false);
var storageClient = await InstanceHelpers.CreateStorageClientAsync().ConfigureAwait(false);
var pubSubClient = await InstanceHelpers.CreatePubSubClientAsync().ConfigureAwait(false);
var redis = await builder.CreateRedisAsync().ConfigureAwait(false);
var rabbit = await builder.CreateRabbitAsync().ConfigureAwait(false);
var spannerConnection = builder.Configuration.CreateSpannerConnection();

builder.Services.AddSingleton(firestoreDbs["my-gcp-project-id"]);
builder.Services.AddKeycloak(builder.Configuration);
builder.Services.AddGotenberg();
builder.Services.AddScoped<SpannerConnection>(_ => spannerConnection);
builder.Services.AddSingleton(pubSubClient);
builder.Services.AddSingleton(storageClient);
builder.Services.AddSingleton(builder.CreateSmtp());
builder.Services.AddSingleton(redis);
builder.Services.AddSingleton(rabbit);
builder.Services.AddScoped<ISpannerService, SpannerService>();
builder.Services.AddScoped<IStorageService, GoogleCloudStorageAdapter>();
builder.Services.AddScoped<IMessagePublisher, GooglePubSubMessagePublisher>();
builder.Services.AddScoped<IGotenbergService, GotenbergService>();
builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformer>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
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
