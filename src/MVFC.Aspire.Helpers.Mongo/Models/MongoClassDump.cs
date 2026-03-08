namespace MVFC.Aspire.Helpers.Mongo.Models;

/// <summary>
/// Represents a MongoDB collection "dump" containing information about the database,
/// collection, number of documents, and a fake data generator for the documents.
/// </summary>
/// <typeparam name="T">
/// Type of documents that will be generated and included in the dump. Must be a class.
/// </typeparam>
/// <param name="DatabaseName">MongoDB database name associated with the dump.</param>
/// <param name="CollectionName">MongoDB collection name associated with the dump.</param>
/// <param name="Quantity">Number of documents to be generated in the dump.</param>
/// <param name="Faker">Fake data generator instance (<see cref="Faker{T}"/>) for creating the documents.</param>
public record class MongoClassDump<T>(
    string DatabaseName,
    string CollectionName,
    int Quantity,
    Faker<T> Faker) : IMongoClassDump where T : class
{
    /// <inheritdoc />
    public async Task ExecuteDumpAsync(IMongoClient client, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        var database = client.GetDatabase(DatabaseName);
        var collection = database.GetCollection<T>(CollectionName);
        var items = Faker.Generate(Quantity);

        if (items.Count > 0)
        {
            await collection.InsertManyAsync(items, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
