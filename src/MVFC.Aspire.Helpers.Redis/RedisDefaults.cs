namespace MVFC.Aspire.Helpers.Redis;

/// <summary>
/// Default values for Redis configuration.
/// </summary>
public static class RedisDefaults
{
    /// <summary>Default Redis Docker image.</summary>
    public const string DEFAULT_REDIS_IMAGE = "redis";

    /// <summary>Default Redis Docker image tag.</summary>
    public const string DEFAULT_REDIS_TAG = "latest";

    /// <summary>Default Redis Commander Docker image.</summary>
    public const string DEFAULT_COMMANDER_IMAGE = "ghcr.io/joeferner/redis-commander";

    /// <summary>Default Redis Commander Docker image tag.</summary>
    public const string DEFAULT_COMMANDER_TAG = "latest";

    /// <summary>Default Redis port.</summary>
    public const int DEFAULT_REDIS_PORT = 6379;

    /// <summary>Default Redis Commander port.</summary>
    public const int DEFAULT_COMMANDER_PORT = 8081;

    /// <summary>Default Redis connection string section in application configuration.</summary>
    public const string DEFAULT_CONNECTION_STRING_SECTION = "ConnectionStrings:redis";

    /// <summary>Environment variable for the Redis password.</summary>
    public const string PASSWORD_ENV_VAR = "REDIS_PASSWORD";

    /// <summary>Environment variable for the Redis Commander host.</summary>
    public const string HOST_ENV_VAR = "REDIS_HOST";

    /// <summary>Environment variable for the Redis Commander port.</summary>
    public const string PORT_ENV_VAR = "REDIS_PORT";

    /// <summary>Command-line argument to set the Redis required password.</summary>
    public const string REQUIRE_PASS_ARG = "--requirepass";

    /// <summary>Command-line argument to enable AOF persistence in Redis.</summary>
    public const string APPEND_ONLY_ARG = "--appendonly";

    /// <summary>AOF argument value to enable persistence.</summary>
    public const string YES_ARG = "yes";

    /// <summary>Persistent data volume path for Redis in the container.</summary>
    public const string DATA_VOLUME_PATH = "/data";

    /// <summary>HTTP endpoint name for Redis Commander.</summary>
    public const string COMMANDER_HTTP_ENDPOINT_NAME = "http";

    /// <summary>Redis endpoint name.</summary>
    public const string ENDPOINT_NAME = "redis";
}
