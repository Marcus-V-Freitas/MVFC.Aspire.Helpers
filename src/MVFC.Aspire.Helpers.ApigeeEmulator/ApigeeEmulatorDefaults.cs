namespace MVFC.Aspire.Helpers.ApigeeEmulator;

/// <summary>
/// Default configuration constants for the Apigee Emulator.
/// </summary>
internal static class ApigeeEmulatorDefaults
{
    // Docker image
    /// <summary>
    /// Container registry containing the Apigee emulator image.
    /// </summary>
    internal const string DEFAULT_IMAGE_REGISTRY = "gcr.io/apigee-release/hybrid";

    /// <summary>
    /// Default Apigee emulator container image.
    /// </summary>
    internal const string DEFAULT_IMAGE = "apigee-emulator";

    /// <summary>
    /// Default Apigee emulator container image tag.
    /// </summary>
    internal const string DEFAULT_IMAGE_TAG = "1.12.4";

    // Ports — host-side defaults
    /// <summary>
    /// Default host port for the emulator control interface.
    /// </summary>
    internal const int DEFAULT_CONTROL_PORT = 7071;

    /// <summary>
    /// Default host port for proxy traffic.
    /// </summary>
    internal const int DEFAULT_TRAFFIC_PORT = 8998;

    // Ports — container-internal targets
    /// <summary>
    /// Container port serving the emulator control interface.
    /// </summary>
    internal const int CONTROL_TARGET_PORT = 8080;

    /// <summary>
    /// Container port serving proxy traffic.
    /// </summary>
    internal const int TRAFFIC_TARGET_PORT = 8998;

    // Apigee environment
    /// <summary>
    /// Default environment name used for local proxy deployment.
    /// </summary>
    internal const string DEFAULT_ENVIRONMENT = "local";

    // Docker DNS for host machine access from container
    /// <summary>
    /// Docker host name used to reach backend services from the emulator container.
    /// </summary>
    internal const string DOCKER_INTERNAL_HOST = "host.docker.internal";

    // Emulator readiness check
    /// <summary>
    /// Control interface path used to check emulator readiness.
    /// </summary>
    internal const string EMULATOR_READY_PATH = "/v1/emulator/tree";

    /// <summary>
    /// Maximum number of emulator readiness checks.
    /// </summary>
    internal const int EMULATOR_READY_MAX_RETRIES = 60;

    /// <summary>
    /// Delay between emulator readiness checks, in seconds.
    /// </summary>
    internal const int EMULATOR_READY_DELAY_SECONDS = 1;

    /// <summary>
    /// Environment variable controlling the HTTP addresses bound by an ASP.NET Core backend.
    /// </summary>
    internal const string BACKEND_URLS_ENV_VAR = "ASPNETCORE_URLS";

    // Proxy readiness check (after deploy)
    /// <summary>
    /// Maximum number of proxy readiness checks after deployment.
    /// </summary>
    internal const int PROXY_READY_MAX_RETRIES = 30;

    /// <summary>
    /// Delay between proxy readiness checks, in seconds.
    /// </summary>
    internal const int PROXY_READY_DELAY_SECONDS = 3;
}
