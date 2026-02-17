namespace MVFC.Aspire.Helpers.Mongo.Processors;

/// <summary>
/// Implementação do processador de dumps de coleções MongoDB
/// </summary>
internal sealed class MongoDumpProcessor : IMongoDumpProcessor
{
    private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MongoDumpProcessor>();

    public async Task ProcessDumpsAsync(
        IMongoClient client,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken)
    {
        var tasks = dumps.Select(dump => ProcessSingleDumpAsync(client, dump, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private static async Task ProcessSingleDumpAsync(
        IMongoClient client,
        IMongoClassDump dump,
        CancellationToken cancellationToken)
    {
        try
        {
            await dump.ExecuteDumpAsync(client, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogMongoDumpUnexpectedError(ex, dump.DatabaseName, dump.CollectionName);
        }
    }
}
