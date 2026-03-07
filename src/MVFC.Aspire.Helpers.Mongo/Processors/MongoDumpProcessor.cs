namespace MVFC.Aspire.Helpers.Mongo.Processors;

/// <summary>
/// MongoDB collection dump processor implementation.
/// </summary>
internal static class MongoDumpProcessor
{
    /// <summary>
    /// Executes all dumps in parallel using the provided MongoDB client.
    /// </summary>
    internal static async Task ProcessDumpsAsync(
        IMongoClient client,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        var tasks = dumps.Select(dump => dump.ExecuteDumpAsync(client, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
