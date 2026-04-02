namespace MVFC.Aspire.Helpers.WireMock;

/// <summary>
/// Extension methods to simplify WireMock integration with Aspire,
/// including endpoint creation and WireMock resource registration in the distributed application.
/// </summary>
public static class WireMockExtensions
{
    /// <summary>
    /// Creates an <see cref="EndpointBuilder"/> for the specified path, allowing configuration of mocked endpoints.
    /// </summary>
    /// <param name="server">WireMock server instance.</param>
    /// <param name="path">Endpoint path to be configured.</param>
    /// <returns>An <see cref="EndpointBuilder"/> instance for endpoint configuration.</returns>
    public static EndpointBuilder Endpoint(this WireMockServer server, string path)
        => new(server, path);

    /// <summary>
    /// Adds a WireMock resource to the Aspire distributed application, registering the lifecycle hook and configuring the endpoint.
    /// </summary>
    /// <param name="builder">Distributed application builder.</param>
    /// <param name="name">WireMock resource name.</param>
    /// <param name="port">TCP port for the WireMock server. Default: 8080.</param>
    /// <param name="configure">Optional action for additional WireMock server configuration.</param>
    /// <returns>A resource builder for the <see cref="WireMockResource"/>.</returns>
    public static IResourceBuilder<WireMockResource> AddWireMock(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = WireMockDefaults.DEFAULT_PORT,
        Action<WireMockServer>? configure = null)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new WireMockResource(name, port, configure);

        // Register eventing subscriber
        builder.Services.TryAddEventingSubscriber<WireMockLifecycleHook>();

        return builder.AddResource(resource)
                      .WithInitialState(new CustomResourceSnapshot
                      {
                          Properties =
                          [
                              new(CustomResourceKnownProperties.Source, WireMockDefaults.RESOURCE_SOURCE_PROPERTY)
                          ],
                          ResourceType = WireMockDefaults.RESOURCE_TYPE_PROPERTY,
                          CreationTimeStamp = DateTimeOffset.UtcNow.DateTime,
                          State = KnownResourceStates.NotStarted,
                      });
    }
}
