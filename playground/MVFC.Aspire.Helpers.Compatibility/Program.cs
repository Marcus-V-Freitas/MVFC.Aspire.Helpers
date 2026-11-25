var builder = DistributedApplication.CreateBuilder(args);
var mongo = builder.AddMongoReplicaSet("mongo");
var redis = builder.AddRedis("redis").WithPassword("smoke-secret");
var rabbit = builder.AddRabbitMQ("rabbit");
var spanner = builder.AddGcpSpanner("spanner");
var wireMock = builder.AddWireMock("wiremock", port: null);
using var server = wireMock.Resource.Server;
var consumer = builder.AddProject("consumer", Path.GetFullPath("playground/MVFC.Aspire.Helpers.Compatibility/MVFC.Aspire.Helpers.Compatibility.csproj"))
    .WithReference(source: mongo)
    .WithReference(redis)
    .WithReference(source: rabbit)
    .WithReference(source: spanner);
rabbit.WithCredentials("smoke-user", "smoke @:/%secret");
redis.WithPassword("smoke-secret,with-comma");

foreach (var resource in new IResource[] { mongo.Resource, redis.Resource, rabbit.Resource, spanner.Resource })
{
    foreach (var endpoint in resource.Annotations.OfType<EndpointAnnotation>())
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", endpoint.TargetPort!.Value);
}

var environment = new Dictionary<string, object>();
var context = new EnvironmentCallbackContext(builder.ExecutionContext, consumer.Resource, environment);
foreach (var callback in consumer.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
    await callback.Callback(context);

foreach (var name in new[] { "mongo", "redis", "rabbit", "spanner" })
{
    if (!environment.TryGetValue($"ConnectionStrings__{name}", out var expression) ||
        expression is not IValueProvider provider ||
        string.IsNullOrWhiteSpace(await provider.GetValueAsync(default)))
        throw new InvalidOperationException($"Package consumer could not resolve {name}.");
}

var password = await ((IValueProvider)environment["Redis__redis__Password"]).GetValueAsync(default);
if (password != "smoke-secret,with-comma")
    throw new InvalidOperationException("The Redis package consumer did not receive the final password setting.");
var amqp = await rabbit.Resource.ConnectionStringExpression.GetValueAsync(default);
if (amqp != "amqp://smoke-user:smoke%20%40%3A%2F%25secret@localhost:5672")
    throw new InvalidOperationException("AMQP credentials were not preserved by package consumption.");

if (!server.IsStarted)
    throw new InvalidOperationException("WireMock package did not start its server.");

Console.WriteLine($"Package consumption verified on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}.");
