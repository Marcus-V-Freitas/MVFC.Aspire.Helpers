namespace MVFC.Aspire.Helpers.GcpPubSub.Resources;

/// <summary>
/// Represents the Google Pub/Sub emulator admin UI resource.
/// </summary>
public sealed class PubSubUIResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Gets the reference to the HTTP endpoint of the UI.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);

    /// <summary>
    /// Expression that builds the connection string for the UI.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Property(EndpointProperty.Scheme)}://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );
}
