namespace MVFC.Aspire.Helpers.Mongo.Models;

/// <summary>
/// Replaces or inserts generated documents by their serialized, stable MongoDB _id.
/// Existing documents absent from the generated batch are retained. This dump is opt-in;
/// <see cref="MongoClassDump{T}"/> continues to append documents.
/// </summary>
/// <typeparam name="T">Document type whose serializer supplies a non-null _id.</typeparam>
/// <param name="DatabaseName">Target database.</param>
/// <param name="CollectionName">Target collection.</param>
/// <param name="Quantity">Number of documents to generate.</param>
/// <param name="Faker">Generator that must supply the same identities on repeated executions.</param>
public sealed record MongoUpsertDump<T>(
    string DatabaseName,
    string CollectionName,
    int Quantity,
    Faker<T> Faker) : IMongoClassDump where T : class
{
    /// <summary>
    /// Validates all generated identities before writing full-document replacements.
    /// A failed or canceled batch can leave partial writes; reexecution reconciles the same identities.
    /// </summary>
    public async Task ExecuteDumpAsync(IMongoClient client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidateConfiguration(DatabaseName, CollectionName, Quantity, Faker);
        cancellationToken.ThrowIfCancellationRequested();

        var writes = CreateWrites(cancellationToken);

        if (writes.Count == 0)
            return;

        var collection = client.GetDatabase(DatabaseName).GetCollection<T>(CollectionName);
        await collection.BulkWriteAsync(writes, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rejects invalid configuration before generating or writing documents.
    /// </summary>
    private static void ValidateConfiguration(string databaseName, string collectionName, int quantity, Faker<T> faker)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ArgumentNullException.ThrowIfNull(faker);
    }

    /// <summary>
    /// Builds replacements after checking serialized identities and duplicate keys in the batch.
    /// </summary>
    private List<WriteModel<T>> CreateWrites(CancellationToken cancellationToken)
    {
        var identities = new HashSet<BsonValue>();
        var writes = new List<WriteModel<T>>(Quantity);

        foreach (var item in Faker.Generate(Quantity))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = item.ToBsonDocument();

            if (!document.TryGetValue("_id", out var id) || id.IsBsonNull)
                throw new InvalidOperationException("Upsert seed documents must supply a stable, non-null _id.");

            if (!identities.Add(id))
                throw new InvalidOperationException("Upsert seed documents must have distinct _id values within the batch.");

            writes.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Eq("_id", id), item) { IsUpsert = true });
        }

        return writes;
    }
}
