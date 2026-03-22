namespace MVFC.Aspire.Helpers.GcpSpanner;

/// <summary>
/// Default configuration constants for the Google Cloud Spanner emulator.
/// </summary>
internal static class SpannerDefaults
{
    // Ports — emulador expõe gRPC na 9010 e REST na 9020
    internal const int GRPC_PORT = 9010;

    // Docker image
    internal const string EMULATOR_IMAGE = "gcr.io/cloud-spanner-emulator/emulator";
    internal const string EMULATOR_IMAGE_TAG = "latest";

    // Env var que o SDK do Spanner respeita para apontar ao emulador (formato host:port gRPC)
    internal const string EMULATOR_HOST_ENV_VAR = "SPANNER_EMULATOR_HOST";

    // Timeouts
    internal const int WAIT_TIMEOUT_SECONDS_DEFAULT = 30;

    // Instance config name exigida pelo emulador
    internal const string EMULATOR_INSTANCE_CONFIG = "emulator-config";
}
