namespace MVFC.Aspire.Helpers.Keycloak.Configs;

/// <summary>
/// Configuration for a Keycloak Group.
/// </summary>
/// <param name="Name">The name of the group.</param>
/// <param name="RealmRoles">An optional list of realm roles to assign to the group.</param>
public sealed record KeycloakGroupConfig(
    string Name,
    IReadOnlyList<string>? RealmRoles = null);
