namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Constantes de configuração para MongoDB Replica Set
/// </summary>
internal static class MongoDefaults
{
    public const int ContainerPort = 27017;
    public const int DefaultTimeoutInSeconds = 300;
    public const string DefaultMongoImage = "mongo";
    public const string DefaultImageTag = "latest";
    public const string DefaultConnectionStringSection = "ConnectionStrings:mongo";
    public const string ReplicaSetName = "rs0";
    public const string EndpointName = "mongodb";
}
