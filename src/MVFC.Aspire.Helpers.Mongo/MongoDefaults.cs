namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Default configuration constants for MongoDB Replica Set.
/// </summary>
internal static class MongoDefaults
{
    /// <summary>Default MongoDB container port.</summary>
    public const int HOST_PORT = 27017;

    /// <summary>Default timeout in seconds for MongoDB connection and server selection.</summary>
    public const int DEFAULT_TIMEOUT_IN_SECONDS = 300;

    /// <summary>Default MongoDB Docker image.</summary>
    public const string DEFAULT_MONGO_IMAGE = "mongo";

    /// <summary>Default MongoDB Docker image tag.</summary>
    public const string DEFAULT_IMAGE_TAG = "latest";

    /// <summary>Default MongoDB connection string section in application configuration.</summary>
    public const string DEFAULT_CONNECTION_STRING_SECTION = "ConnectionStrings:mongo";

    /// <summary>Replica Set name used by the MongoDB emulator.</summary>
    public const string REPLICA_SET_NAME = "rs0";

    /// <summary>MongoDB endpoint name exposed by the container.</summary>
    public const string ENDPOINT_NAME = "mongodb";

    /// <summary>MongoDB data volume path in the container.</summary>
    public const string DATA_VOLUME_PATH = "/data/db";

    /// <summary>MongoDB initialization scripts path in the container.</summary>
    public const string INIT_SCRIPTS_PATH = "/docker-entrypoint-initdb.d";

    /// <summary>Replica Set initialization script filename.</summary>
    public const string REPLICA_SET_INIT_SCRIPT_FILENAME = "init-replica-set.js";

    /// <summary>Argument to accept connections from any IP address.</summary>
    public const string BIND_IP_ALL_ARG = "--bind_ip_all";

    /// <summary>Argument to configure the Replica Set name.</summary>
    public const string REPL_SET_ARG = "--replSet";
}
