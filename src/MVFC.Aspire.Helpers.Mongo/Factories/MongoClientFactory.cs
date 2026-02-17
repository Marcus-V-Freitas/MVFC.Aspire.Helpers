namespace MVFC.Aspire.Helpers.Mongo.Factories;

/// <summary>
/// Implementação da factory de clientes MongoDB
/// </summary>
internal sealed class MongoClientFactory : IMongoClientFactory
{
    private readonly IMongoDumpProcessor _dumpProcessor = new MongoDumpProcessor();

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, IMongoClient> _clients = new();

    public async Task ExecuteDumpsAsync(
        string connectionString,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(dumps);

        if (dumps.Count == 0)
        {
            return;
        }

        var client = _clients.GetOrAdd(connectionString, CreateClient);

        await _dumpProcessor.ProcessDumpsAsync(client, dumps, cancellationToken);
    }

    private static IMongoClient CreateClient(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ConnectTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(MongoDefaults.DefaultTimeoutInSeconds);
        return new MongoClient(settings);
    }
}
