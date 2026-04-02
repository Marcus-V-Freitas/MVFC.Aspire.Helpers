namespace MVFC.Aspire.Helpers.Redis;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of the Redis resource
/// in distributed applications using Aspire.
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Adds a Redis resource to the distributed application with default settings.
    /// Use fluent methods such as WithPassword, WithCommander, WithDataVolume to customize.
    /// </summary>
    public static IResourceBuilder<RedisResource> AddRedis(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = RedisDefaults.DEFAULT_REDIS_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new RedisResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: RedisDefaults.DEFAULT_REDIS_IMAGE,
                tag: RedisDefaults.DEFAULT_REDIS_TAG)
            .WithRedisEndpoint(port);
    }

    /// <summary>
    /// Replaces the Docker image used by the Redis resource.
    /// </summary>
    public static IResourceBuilder<RedisResource> WithDockerImage(
        this IResourceBuilder<RedisResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Configures authentication password for Redis.
    /// </summary>
    public static IResourceBuilder<RedisResource> WithPassword(
        this IResourceBuilder<RedisResource> builder,
        string password)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(RedisDefaults.PASSWORD_ENV_VAR, password)
                      .WithArgs(RedisDefaults.REQUIRE_PASS_ARG, password);
    }

    /// <summary>
    /// Adds Redis Commander UI as an associated container.
    /// </summary>
    public static IResourceBuilder<RedisResource> WithCommander(
        this IResourceBuilder<RedisResource> builder,
        int? port = null,
        string image = RedisDefaults.DEFAULT_COMMANDER_IMAGE,
        string tag = RedisDefaults.DEFAULT_COMMANDER_TAG)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        var commanderName = $"{builder.Resource.Name}-commander";
        builder.ApplicationBuilder.AddContainer(commanderName, image, tag)
            .WithHttpEndpoint(
                port: port,
                targetPort: RedisDefaults.DEFAULT_COMMANDER_PORT,
                name: RedisDefaults.COMMANDER_HTTP_ENDPOINT_NAME)
            .WithEnvironment(RedisDefaults.HOST_ENV_VAR, builder.Resource.Name)
            .WithEnvironment(RedisDefaults.PORT_ENV_VAR, RedisDefaults.DEFAULT_REDIS_PORT.ToString(CultureInfo.InvariantCulture))
            .WithReference(builder);

        return builder;
    }

    /// <summary>
    /// Configures a Docker volume for Redis data persistence with AOF enabled.
    /// </summary>
    public static IResourceBuilder<RedisResource> WithDataVolume(
        this IResourceBuilder<RedisResource> builder,
        string volumeName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(volumeName);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(volumeName, RedisDefaults.DATA_VOLUME_PATH)
                      .WithArgs(RedisDefaults.APPEND_ONLY_ARG, RedisDefaults.YES_ARG);
    }

    /// <summary>
    /// Configures the Redis endpoint on the container, without proxy.
    /// </summary>
    private static IResourceBuilder<RedisResource> WithRedisEndpoint(
        this IResourceBuilder<RedisResource> resource,
        int port)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource.WithEndpoint(
                port: port,
                targetPort: RedisDefaults.DEFAULT_REDIS_PORT,
                name: RedisDefaults.ENDPOINT_NAME,
                isProxied: false);
    }
}
