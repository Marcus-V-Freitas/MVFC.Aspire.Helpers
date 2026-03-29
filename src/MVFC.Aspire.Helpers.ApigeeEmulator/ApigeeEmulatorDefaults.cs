namespace MVFC.Aspire.Helpers.ApigeeEmulator;

/// <summary>
/// Default configuration constants for the Apigee Emulator.
/// </summary>
internal static class ApigeeEmulatorDefaults
{
    // Docker image
    internal const string DEFAULT_IMAGE_REGISTRY = "gcr.io/apigee-release/hybrid";
    internal const string CONTAINER_BUNDLE_PATH = "/bundle/proxy.zip";
    internal const string DEFAULT_IMAGE = "apigee-emulator";
    internal const string DEFAULT_IMAGE_TAG = "1.12.4";

    // Ports — host-side defaults
    internal const int DEFAULT_CONTROL_PORT = 7071;
    internal const int DEFAULT_TRAFFIC_PORT = 8998;

    // Ports — container-internal targets
    internal const int CONTROL_TARGET_PORT = 8080;
    internal const int TRAFFIC_TARGET_PORT = 8998;

    // Apigee environment
    internal const string DEFAULT_ENVIRONMENT = "local";

    // Docker DNS for host machine access from container
    internal const string DOCKER_INTERNAL_HOST = "host.docker.internal";

    // Default TargetServer name used in targets/default.xml
    internal const string DEFAULT_TARGET_SERVER_NAME = "aspire-backend";

    // Emulator readiness check
    internal const string EMULATOR_READY_PATH = "/v1/emulator/tree";
    internal const int EMULATOR_READY_MAX_RETRIES = 60;
    internal const int EMULATOR_READY_DELAY_SECONDS = 1;

    // Proxy readiness check (after deploy)
    internal const int PROXY_READY_MAX_RETRIES = 60;
    internal const int PROXY_READY_DELAY_SECONDS = 1;
}
