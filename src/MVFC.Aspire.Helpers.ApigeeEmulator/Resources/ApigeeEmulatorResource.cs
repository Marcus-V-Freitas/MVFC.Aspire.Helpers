namespace MVFC.Aspire.Helpers.ApigeeEmulator.Resources;

/// <summary>
/// Represents the Apigee Emulator container resource for use in distributed applications.
/// </summary>
public sealed class ApigeeEmulatorResource(string name) : ContainerResource(name)
{
    internal const string CONTROL_PORT_NAME = "control";
    internal const string TRAFFIC_PORT_NAME = "http";

    internal string? WorkspacePath { get; set; }

    public string? HealthCheckPath { get; set; }

    internal string ApigeeEnvironment { get; set; } = ApigeeEmulatorDefaults.DEFAULT_ENVIRONMENT;

    /// <summary>
    /// Caminho do ZIP pré-construído em tempo de alocação de endpoints.
    /// Evita rebuild durante o deploy pós-startup.
    /// </summary>
    internal string? PrebuiltBundlePath { get; set; }
}
