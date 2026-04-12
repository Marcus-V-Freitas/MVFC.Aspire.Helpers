namespace MVFC.Aspire.Helpers.GcpFirestore.Resources;

/// <summary>
/// Represents the Google Firestore emulator resource for use in distributed applications.
/// </summary>
public sealed class FirestoreEmulatorResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>Internal HTTP endpoint name for the Firestore emulator.</summary>
    internal const string HTTP_ENDPOINT_NAME = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Gets the reference to the HTTP endpoint of the emulator.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HTTP_ENDPOINT_NAME);

    /// <summary>
    /// Expression that builds the connection string for the Firestore emulator (host:port).
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// Firestore project configurations.
    /// </summary>
    internal IReadOnlyList<FirestoreConfig> FirestoreConfigs { get; set; } = [];
}