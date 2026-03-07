namespace MVFC.Aspire.Helpers.CloudStorage.Resources;

/// <summary>
/// Represents a Cloud Storage resource (GCS emulator) for use in distributed applications,
/// providing access to the HTTP endpoint and connection expression for integration.
/// </summary>
public sealed class CloudStorageResource(string name)
    : ContainerResource(name), IResourceWithConnectionString
{

    /// <summary>
    /// Default name of the HTTP endpoint exposed by the Cloud Storage resource.
    /// </summary>
    internal const string HTTP_ENDPOINT_NAME = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Gets the reference to the HTTP endpoint of the Cloud Storage resource.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HTTP_ENDPOINT_NAME);

    /// <summary>
    /// Expression that builds the connection string for the Cloud Storage emulator endpoint,
    /// including the scheme, host, port, and the "/storage/v1/" suffix.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Property(EndpointProperty.Scheme)}://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}/storage/v1/"
        );
}
