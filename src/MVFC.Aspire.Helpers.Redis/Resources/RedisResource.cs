namespace MVFC.Aspire.Helpers.Redis.Resources;

/// <summary>
/// Represents the Redis resource as an Aspire container, providing a Redis endpoint
/// and a connection string expression for integration in distributed applications.
/// </summary>
/// <remarks>
/// This class encapsulates the endpoint configuration required for Redis operation,
/// enabling easy referencing and integration with other Aspire resources.
/// </remarks>
public sealed class RedisResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    private EndpointReference? _redisReference;

    /// <summary>
    /// Reference to the Redis endpoint of the resource.
    /// </summary>
    public EndpointReference RedisEndpoint =>
        _redisReference ??= new(this, RedisDefaults.ENDPOINT_NAME);

    /// <summary>
    /// Expression representing the Redis connection string.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{RedisEndpoint.Property(EndpointProperty.Host)}:{RedisEndpoint.Property(EndpointProperty.Port)}"
        );
}
