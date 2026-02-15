namespace MVFC.Aspire.Helpers.Mongo.Processors;

/// <summary>
/// Implementação do processador de dumps de coleções MongoDB
/// </summary>
internal sealed class MongoDumpProcessor : IMongoDumpProcessor {
    private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MongoDumpProcessor>();

    public async Task ProcessDumpsAsync(
        IMongoClient client,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken) {
        var tasks = dumps.Select(dump => ProcessSingleDumpAsync(client, dump, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private static async Task ProcessSingleDumpAsync(
        IMongoClient client,
        IMongoClassDump dump,
        CancellationToken cancellationToken) {
        try {
            var genericType = ExtractGenericType(dump);
            var dumpMethod = GetGenericDumpMethod(genericType);

            var taskResult = dumpMethod.Invoke(null, [client, dump, cancellationToken]);

            if (taskResult is Task task) {
                await task;
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException is MongoException mongoEx) {
            _logger.LogMongoDumpFailed(mongoEx, dump.DatabaseName, dump.CollectionName);
        }
        catch (Exception ex) {
            _logger.LogMongoDumpUnexpectedError(ex, dump.DatabaseName, dump.CollectionName);
        }
    }

    private static Type ExtractGenericType(IMongoClassDump dump) {
        var dumpType = dump.GetType();
        var genericArgs = dumpType.GetGenericArguments();

        return genericArgs.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Generic type not found for {dumpType.Name}");
    }

    private static MethodInfo GetGenericDumpMethod(Type genericType) {
        var method = typeof(MongoDumpProcessor).GetMethod(
            nameof(DumpCollectionAsync),
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(
                $"Method {nameof(DumpCollectionAsync)} not found");

        return method.MakeGenericMethod(genericType);
    }

    private static async Task DumpCollectionAsync<T>(
        IMongoClient client,
        MongoClassDump<T> dump,
        CancellationToken cancellationToken)
        where T : class {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dump);

        var database = client.GetDatabase(dump.DatabaseName);
        var collection = database.GetCollection<T>(dump.CollectionName);
        var items = dump.Faker.Generate(dump.Quantity);

        await collection.InsertManyAsync(items, cancellationToken: cancellationToken);
    }
}
