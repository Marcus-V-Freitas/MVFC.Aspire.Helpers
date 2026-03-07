namespace MVFC.Aspire.Helpers.Mongo.Resources;

/// <summary>
/// Represents a MongoDB resource configured as a Replica Set for use in distributed applications,
/// providing access to the MongoDB endpoint and connection expression for integration.
/// </summary>
public sealed class MongoReplicaSetResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>
    /// Default name of the MongoDB endpoint exposed by the resource.
    /// </summary>
    internal const string MONGO_ENDPOINT_NAME = "mongodb";

    private EndpointReference? _mongoReference;

    /// <summary>
    /// Gets the reference to the MongoDB endpoint of the resource.
    /// </summary>
    public EndpointReference MongoEndpoint =>
        _mongoReference ??= new(this, MONGO_ENDPOINT_NAME);

    /// <summary>
    /// Expression that builds the connection string for the MongoDB Replica Set,
    /// using directConnection=true to avoid redirection to the internal hostname.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"mongodb://{MongoEndpoint.Property(EndpointProperty.Host)}:{MongoEndpoint.Property(EndpointProperty.Port)}/?directConnection=true"
        );

    /// <summary>
    /// Data dumps to be executed when the resource is referenced by projects.
    /// </summary>
    internal IReadOnlyCollection<IMongoClassDump>? Dumps { get; set; }
}
