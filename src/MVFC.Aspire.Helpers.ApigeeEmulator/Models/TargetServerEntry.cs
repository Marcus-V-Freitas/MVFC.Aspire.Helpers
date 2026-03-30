namespace MVFC.Aspire.Helpers.ApigeeEmulator.Models;

/// <summary>
/// Represents a Target Server entry for Apigee Emulator backend configuration.
/// Used to map a logical target server name to a specific host and port,
/// allowing Apigee proxies to route requests to the correct backend during development.
/// </summary>
/// <param name="Name">
/// Logical name of the Target Server (e.g., "aspire-backend").
/// This name is referenced in Apigee proxy configurations.
/// </param>
/// <param name="Host">
/// Hostname or IP address of the backend target (e.g., "localhost" or "api").
/// </param>
/// <param name="Port">
/// TCP port of the backend target.
/// </param>
internal sealed record TargetServerEntry(
    string Name,
    string Host,
    int Port);
