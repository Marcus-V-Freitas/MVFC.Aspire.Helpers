namespace MVFC.Aspire.Helpers.Mongo.Factories;

/// <summary>
/// Implementação da factory de clientes MongoDB
/// </summary>
internal sealed class MongoClientFactory : IMongoClientFactory {
    private readonly IMongoDumpProcessor _dumpProcessor = new MongoDumpProcessor();

    public async Task ExecuteDumpsAsync(
        string connectionString,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(dumps);

        if (dumps.Count == 0) {
            return;
        }

        var settings = BuildMongoClientSettings(connectionString);
        using var client = new MongoClient(settings);

        await _dumpProcessor.ProcessDumpsAsync(client, dumps, cancellationToken);
    }

    private static MongoClientSettings BuildMongoClientSettings(string connectionString) {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ConnectTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        return settings;
    }
}
