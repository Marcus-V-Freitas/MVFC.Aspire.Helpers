namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo.Helpers;

internal sealed class NoOpMongoDump(string database, string collection, int quantity) : IMongoClassDump
{
    public string DatabaseName { get; } = database;
    public string CollectionName { get; } = collection;
    public int Quantity { get; } = quantity;

    public async Task ExecuteDumpAsync(IMongoClient client, CancellationToken cancellationToken)
        => await Task.CompletedTask.ConfigureAwait(true);
}
