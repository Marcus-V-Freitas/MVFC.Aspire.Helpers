namespace MVFC.Aspire.Helpers.Mongo.Annotations;

/// <summary>
/// Marks a consumer that waits for dumps of a specific MongoDB resource before starting.
/// </summary>
internal sealed class MongoDumpsWaitAnnotation(MongoReplicaSetResource resource) : IResourceAnnotation
{
    /// <summary>
    /// Gets the MongoDB resource whose dumps must complete.
    /// </summary>
    internal MongoReplicaSetResource Resource { get; } = resource;
}
