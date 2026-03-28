namespace MVFC.Aspire.Helpers.ApigeeEmulator.Annotations;

/// <summary>
/// Annotation that links an Aspire backend resource as a TargetServer for the Apigee proxy.
/// </summary>
internal sealed class ApigeeTargetBackendAnnotation : IResourceAnnotation
{
    /// <summary>Backend resource that exposes endpoints.</summary>
    public required IResource Backend { get; init; }

    /// <summary>Name of the endpoint exposed by the Aspire resource (e.g., "http").</summary>
    public required string EndpointName { get; init; }

    /// <summary>
    /// TargetServer name referenced in targets/default.xml of the proxy.
    /// </summary>
    public string TargetServerName { get; init; } = ApigeeEmulatorDefaults.DEFAULT_TARGET_SERVER_NAME;
}
