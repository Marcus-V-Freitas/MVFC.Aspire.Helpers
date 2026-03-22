namespace MVFC.Aspire.Helpers.GcpSpanner.Resources;

/// <summary>
/// Represents the Google Cloud Spanner emulator resource for use in distributed applications.
/// </summary>
public sealed class SpannerEmulatorResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>Internal gRPC endpoint name (porta 9010).</summary>
    internal const string GRPC_ENDPOINT_NAME = "grpc";

    private EndpointReference? _grpcReference;

    /// <summary>
    /// Gets the reference to the gRPC endpoint of the emulator.
    /// </summary>
    public EndpointReference GrpcEndpoint =>
        _grpcReference ??= new(this, GRPC_ENDPOINT_NAME);

    /// <summary>
    /// Connection string no formato host:port gRPC — usado pela env var SPANNER_EMULATOR_HOST.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{GrpcEndpoint.Property(EndpointProperty.Host)}:{GrpcEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// Spanner instance + database configurations to be provisioned after the emulator starts.
    /// </summary>
    internal IReadOnlyList<SpannerConfig> SpannerConfigs { get; set; } = [];

    /// <summary>
    /// Wait timeout for emulator initialization in seconds.
    /// </summary>
    internal int WaitTimeoutSeconds { get; set; } = SpannerDefaults.WAIT_TIMEOUT_SECONDS_DEFAULT;
}
