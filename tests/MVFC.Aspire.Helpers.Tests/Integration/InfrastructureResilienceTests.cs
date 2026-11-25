namespace MVFC.Aspire.Helpers.Tests.Integration;

/// <summary>
/// Exercises isolated infrastructure without changing the shared playground fixture.
/// </summary>
public sealed class InfrastructureResilienceTests
{
    [Fact]
    public async Task MongoUpsertDump_ShouldReconcileRepeatedAndConcurrentBatches()
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(2));
        var builder = CreateBuilder();
        builder.AddMongoReplicaSet("seed-mongo", port: 0);
        await using var app = builder.Build();
        await app.StartAsync(cts.Token);
        var connection = await app.GetConnectionStringAsync("seed-mongo", cts.Token);
        var client = new MongoClient(connection);
        var databaseName = $"seed_{Guid.NewGuid():N}";
        var collection = client.GetDatabase(databaseName).GetCollection<BsonDocument>("items");
        var first = CreateDump(databaseName, "before");
        var updated = CreateDump(databaseName, "after");

        try
        {
            // Act
            await first.ExecuteDumpAsync(client, cts.Token);
            await Task.WhenAll(updated.ExecuteDumpAsync(client, cts.Token), CreateDump(databaseName, "after").ExecuteDumpAsync(client, cts.Token));
            var documents = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync(cts.Token);

            // Assert
            documents.Should().ContainSingle();
            documents[0]["_id"].AsString.Should().Be("stable");
            documents[0]["value"].AsString.Should().Be("after");
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task MongoUpsertDump_AfterPartialFailure_ShouldReconcileWrittenDocuments()
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(2));
        var builder = CreateBuilder();
        builder.AddMongoReplicaSet("partial-mongo", port: 0);
        await using var app = builder.Build();
        await app.StartAsync(cts.Token);
        var client = new MongoClient(await app.GetConnectionStringAsync("partial-mongo", cts.Token));
        var databaseName = $"seed_{Guid.NewGuid():N}";

        try
        {
            var partial = client.GetDatabase(databaseName).GetCollection<BsonDocument>("partial");
            await partial.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("unique"), new CreateIndexOptions { Unique = true }), cancellationToken: cts.Token);
            var index = 0;
            var invalid = new MongoUpsertDump<BsonDocument>(databaseName, "partial", 2,
                new Faker<BsonDocument>().CustomInstantiator(_ => new BsonDocument { ["_id"] = index++, ["unique"] = "duplicate" }));
            var fail = () => invalid.ExecuteDumpAsync(client, cts.Token);

            // Act
            await fail.Should().ThrowAsync<MongoBulkWriteException<BsonDocument>>();
            var partialCount = await partial.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cts.Token);
            index = 0;
            var repaired = new MongoUpsertDump<BsonDocument>(databaseName, "partial", 2,
                new Faker<BsonDocument>().CustomInstantiator(_ => new BsonDocument { ["_id"] = index, ["unique"] = $"value-{index++}" }));
            await repaired.ExecuteDumpAsync(client, cts.Token);
            var repairedCount = await partial.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty, cancellationToken: cts.Token);

            // Assert
            partialCount.Should().Be(1);
            repairedCount.Should().Be(2);
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task RabbitMQ_ShouldAuthenticateUsingOnlyReferencedConnection()
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(2));
        var builder = CreateBuilder();
        var rabbit = builder.AddRabbitMQ("authenticated-rabbit", amqpPort: 0, httpPort: 0)
            .WithCredentials("integration-user", "different @:/%,secret");
        await using var app = builder.Build();
        await app.StartAsync(cts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync(rabbit.Resource.Name, cts.Token);
        var connectionString = await app.GetConnectionStringAsync(rabbit.Resource.Name, cts.Token);
        var factory = new RabbitConnectionFactory { Uri = new Uri(connectionString!), AutomaticRecoveryEnabled = false };

        // Act
        await using var connection = await factory.CreateConnectionAsync(cts.Token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cts.Token);
        var queue = await channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true, cancellationToken: cts.Token);

        // Assert
        connection.IsOpen.Should().BeTrue();
        queue.QueueName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("different @:/%,\"雪")]
    public async Task Redis_ShouldConnectUsingReferencedSettings(string? password)
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(2));
        var builder = CreateBuilder();
        var redis = builder.AddRedis("authenticated-redis", port: 0);
        var consumer = builder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("settings-consumer").WithReference(redis);
        builder.Resources.Remove(consumer.Resource);

        if (password is not null)
            redis.WithPassword("old-secret").WithPassword(password);

        await using var app = builder.Build();
        await app.StartAsync(cts.Token);
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, cts.Token);
        var endpoint = await ((IValueProvider)environment["ConnectionStrings__authenticated-redis"]).GetValueAsync(cts.Token);
        var secret = await ((IValueProvider)environment["Redis__authenticated-redis__Password"]).GetValueAsync(cts.Token);
        var options = RedisConfigurationOptions.Parse(endpoint!);
        options.Password = secret;
        options.AbortOnConnectFail = true;

        // Act
        using var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(options);
        var key = $"probe-{Guid.NewGuid():N}";
        var database = connection.GetDatabase();
        await database.StringSetAsync(key, "connected", TimeSpan.FromSeconds(30));
        var result = await database.StringGetAsync(key);
        await database.KeyDeleteAsync(key);

        // Assert
        result.ToString().Should().Be("connected");
        connection.IsConnected.Should().BeTrue();
    }
    /// <summary>
    /// Reuses build metadata for DCP without executing the playground entry point.
    /// </summary>
    private static IDistributedApplicationBuilder CreateBuilder() =>
        DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            AssemblyName = "MVFC.Aspire.Helpers.Playground.AppHost",
            DisableDashboard = true,
            AllowUnsecuredTransport = true,
        });

    /// <summary>
    /// Generates a fixed identity with explicitly replaceable content.
    /// </summary>
    private static MongoUpsertDump<BsonDocument> CreateDump(string databaseName, string value) =>
        new(databaseName, "items", 1, new Faker<BsonDocument>().CustomInstantiator(_ => new BsonDocument
        {
            ["_id"] = "stable",
            ["value"] = value,
        }));
}
