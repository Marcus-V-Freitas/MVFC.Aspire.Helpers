namespace MVFC.Aspire.Helpers.Mongo.Factories;

/// <summary>
/// MongoDB client factory implementation.
/// </summary>
internal static class MongoClientFactory
{
    private static readonly ConcurrentDictionary<string, IMongoClient> _clients = new();

    /// <summary>
    /// Gets or creates a MongoDB client for the provided connection string and executes all configured dumps.
    /// </summary>
    internal static async Task ExecuteDumpsAsync(
        string connectionString,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(dumps);

        var client = _clients.GetOrAdd(connectionString, CreateClient);

        await MongoDumpProcessor.ProcessDumpsAsync(client, dumps, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new <see cref="IMongoClient"/> with configured timeouts from a connection string.
    /// </summary>
    private static IMongoClient CreateClient(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ConnectTimeout = TimeSpan.FromSeconds(MongoDefaults.DEFAULT_TIMEOUT_IN_SECONDS);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(MongoDefaults.DEFAULT_TIMEOUT_IN_SECONDS);
        return new MongoClient(settings);
    }
}
