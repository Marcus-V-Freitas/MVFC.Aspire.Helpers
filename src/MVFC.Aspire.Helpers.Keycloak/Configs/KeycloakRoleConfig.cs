namespace MVFC.Aspire.Helpers.Keycloak.Configs;

/// <summary>
/// Configuration for a Keycloak Role.
/// </summary>
/// <param name="Name">The name of the role.</param>
/// <param name="Description">An optional description for the role.</param>
public sealed record KeycloakRoleConfig(
    string Name,
    string? Description = null);
