namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Constantes de configuração para MongoDB Replica Set
/// </summary>
internal static class MongoDefaults {
    public const int HostPort = 27017;
    public const int DefaultTimeoutInSeconds = 300;
    public const string DefaultMongoImage = "mongo";
    public const string DefaultImageTag = "latest";
    public const string DefaultConnectionString = "mongodb://localhost:27017";
    public const string DefaultConnectionStringSection = "ConnectionStrings:mongo";
    public const string ReplicaSetName = "rs0";
}
