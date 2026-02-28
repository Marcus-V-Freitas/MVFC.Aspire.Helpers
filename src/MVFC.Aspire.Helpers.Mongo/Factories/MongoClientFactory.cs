namespace MVFC.Aspire.Helpers.Mongo.Factories;

/// <summary>
/// Implementação da factory de clientes MongoDB
/// </summary>
internal static class MongoClientFactory
{
    private static readonly ConcurrentDictionary<string, IMongoClient> _clients = new();

    internal static async Task ExecuteDumpsAsync(
        string connectionString,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(dumps);

        var client = _clients.GetOrAdd(connectionString, CreateClient);

        await MongoDumpProcessor.ProcessDumpsAsync(client, dumps, cancellationToken);
    }

    private static IMongoClient CreateClient(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ConnectTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        return new MongoClient(settings);
    }
}
