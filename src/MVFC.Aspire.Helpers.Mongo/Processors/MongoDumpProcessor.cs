namespace MVFC.Aspire.Helpers.Mongo.Processors;

/// <summary>
/// Implementação do processador de dumps de coleções MongoDB
/// </summary>
internal static class MongoDumpProcessor
{
    internal static async Task ProcessDumpsAsync(
        IMongoClient client,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        var tasks = dumps.Select(dump => dump.ExecuteDumpAsync(client, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
