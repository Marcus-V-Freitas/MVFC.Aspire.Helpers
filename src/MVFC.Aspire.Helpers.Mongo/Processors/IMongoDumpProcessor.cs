namespace MVFC.Aspire.Helpers.Mongo.Processors;

/// <summary>
/// Processador responsável por executar dumps de coleções MongoDB
/// </summary>
internal interface IMongoDumpProcessor {
    Task ProcessDumpsAsync(
        IMongoClient client,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken);
}
