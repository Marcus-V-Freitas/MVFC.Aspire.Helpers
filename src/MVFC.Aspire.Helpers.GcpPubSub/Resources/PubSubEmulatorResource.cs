namespace MVFC.Aspire.Helpers.GcpPubSub.Resources;

/// <summary>
/// Represents the Google Pub/Sub emulator resource for use in distributed applications.
/// </summary>
public sealed class PubSubEmulatorResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>Internal HTTP endpoint name for the Pub/Sub emulator.</summary>
    internal const string HTTP_ENDPOINT_NAME = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Gets the reference to the HTTP endpoint of the emulator.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HTTP_ENDPOINT_NAME);

    /// <summary>
    /// Expression that builds the connection string for the Pub/Sub emulator (host:port).
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// Pub/Sub project and topic/subscription configurations.
    /// </summary>
    internal IReadOnlyList<PubSubConfig> PubSubConfigs { get; set; } = [];

    /// <summary>
    /// Wait timeout for emulator initialization in seconds.
    /// </summary>
    internal int WaitTimeoutSeconds { get; set; } = PubSubDefaults.WAIT_TIMEOUT_SECONDS_DEFAULT;
}

/// <summary>
/// Annotation to mark that topics and subscriptions have already been configured.
/// </summary>
internal sealed class PubSubConfiguredAnnotation : IResourceAnnotation
{
}
