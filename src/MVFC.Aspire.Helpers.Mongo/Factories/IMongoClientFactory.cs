namespace MVFC.Aspire.Helpers.Mongo.Factories;

/// <summary>
/// Factory responsável pela criação e configuração de clientes MongoDB
/// </summary>
internal interface IMongoClientFactory {
    Task ExecuteDumpsAsync(
        string connectionString,
        IReadOnlyCollection<IMongoClassDump> dumps,
        CancellationToken cancellationToken);
}
