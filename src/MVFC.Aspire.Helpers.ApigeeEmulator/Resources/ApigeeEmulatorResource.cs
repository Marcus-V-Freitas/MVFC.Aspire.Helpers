namespace MVFC.Aspire.Helpers.ApigeeEmulator.Resources;

/// <summary>
/// Represents the Apigee Emulator container resource for use in distributed applications.
/// </summary>
public sealed class ApigeeEmulatorResource(string name) : ContainerResource(name)
{
    /// <summary>
    /// The name of the control endpoint used for management operations.
    /// </summary>
    internal const string CONTROL_PORT_NAME = "control";

    /// <summary>
    /// The name of the traffic endpoint used for HTTP requests.
    /// </summary>
    internal const string TRAFFIC_PORT_NAME = "http";

    /// <summary>
    /// Gets or sets the path to the workspace directory containing the Apigee proxy bundle.
    /// </summary>
    internal string? WorkspacePath { get; set; }

    /// <summary>
    /// Gets or sets the health check path used to verify the emulator's readiness.
    /// </summary>
    internal string? HealthCheckPath { get; set; }

    /// <summary>
    /// Gets or sets the Apigee environment name for deployment. Defaults to <see cref="ApigeeEmulatorDefaults.DEFAULT_ENVIRONMENT"/>.
    /// </summary>
    internal string ApigeeEnvironment { get; set; } = ApigeeEmulatorDefaults.DEFAULT_ENVIRONMENT;
}
