namespace MVFC.Aspire.Helpers.GcpBigQuery.Resources;

/// <summary>
/// Represents the Google BigQuery emulator resource for use in distributed applications.
/// </summary>
public sealed class BigQueryEmulatorResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>Internal HTTP endpoint name for the BigQuery emulator REST API.</summary>
    internal const string REST_ENDPOINT_NAME = "rest";

    /// <summary>Internal gRPC endpoint name for the BigQuery emulator.</summary>
    internal const string GRPC_ENDPOINT_NAME = "grpc";

    private EndpointReference? _restReference;

    private EndpointReference? _grpcReference;

    /// <summary>
    /// Gets the reference to the REST HTTP endpoint of the emulator.
    /// </summary>
    public EndpointReference RestEndpoint =>
        _restReference ??= new(this, REST_ENDPOINT_NAME);

    /// <summary>
    /// Gets the reference to the gRPC endpoint of the emulator.
    /// </summary>
    public EndpointReference GrpcEndpoint =>
        _grpcReference ??= new(this, GRPC_ENDPOINT_NAME);

    /// <summary>
    /// Expression that builds the connection string for the BigQuery emulator (host:restPort).
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{RestEndpoint.Property(EndpointProperty.Host)}:{RestEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// Expression that builds the connection string for the BigQuery emulator gRPC endpoint (host:grpcPort).
    /// </summary>
    public ReferenceExpression GrpcConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{GrpcEndpoint.Property(EndpointProperty.Host)}:{GrpcEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// BigQuery project configurations.
    /// </summary>
    internal IReadOnlyList<BigQueryConfig> BigQueryConfigs { get; set; } = [];
}
