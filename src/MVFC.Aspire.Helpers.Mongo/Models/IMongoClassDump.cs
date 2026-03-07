namespace MVFC.Aspire.Helpers.Mongo.Models;

/// <summary>
/// Defines the basic structure for a MongoDB collection "dump".
/// Implementations of this interface should provide essential information about
/// the database, collection, and number of documents involved in the operation.
/// </summary>
public interface IMongoClassDump
{
    /// <summary>
    /// MongoDB database name associated with the dump.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// MongoDB collection name associated with the dump.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Number of documents included in the dump.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Executes the document insertion into the specified collection.
    /// </summary>
    /// <param name="client">The MongoDB client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ExecuteDumpAsync(IMongoClient client, CancellationToken cancellationToken);
}
