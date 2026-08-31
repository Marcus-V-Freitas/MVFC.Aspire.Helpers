namespace MVFC.Aspire.Helpers.GcpBigQuery;

/// <summary>
/// Default configuration constants for Google BigQuery Emulator.
/// </summary>
internal static class BigQueryDefaults
{
    /// <summary>
    /// Default internal REST port used by the BigQuery emulator container.
    /// </summary>
    internal const int EMULATOR_REST_PORT = 9050;

    /// <summary>
    /// Default internal gRPC port used by the BigQuery emulator container.
    /// </summary>
    internal const int EMULATOR_GRPC_PORT = 9060;

    /// <summary>
    /// Default external REST port mapped to the BigQuery emulator.
    /// </summary>
    internal const int DEFAULT_EXTERNAL_REST_PORT = 9050;

    /// <summary>
    /// Default external gRPC port mapped to the BigQuery emulator.
    /// </summary>
    internal const int DEFAULT_EXTERNAL_GRPC_PORT = 9060;

    /// <summary>
    /// Official Docker image for the BigQuery emulator.
    /// </summary>
    internal const string EMULATOR_IMAGE = "ghcr.io/goccy/bigquery-emulator";

    /// <summary>
    /// Docker image tag for the BigQuery emulator.
    /// </summary>
    internal const string EMULATOR_IMAGE_TAG = "latest";

    /// <summary>
    /// Environment variable name for the BigQuery emulator host.
    /// </summary>
    public const string EMULATOR_HOST_ENV_VAR = "BIGQUERY_EMULATOR_HOST";

    /// <summary>
    /// Default environment variable name used for the emulator gRPC host.
    /// </summary>
    public const string EMULATOR_GRPC_HOST_ENV_VAR = "BIGQUERY_EMULATOR_GRPC_HOST";
}
