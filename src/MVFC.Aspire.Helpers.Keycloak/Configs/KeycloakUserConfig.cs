namespace MVFC.Aspire.Helpers.Keycloak.Configs;

/// <summary>
/// Configuration for a Keycloak User.
/// </summary>
/// <param name="Username">The internal username for the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="TemporaryPassword">If provided, this password will be set for the user (they might need to change it on first login if temporary).</param>
/// <param name="RealmRoles">An optional list of realm roles to assign to the user.</param>
/// <param name="Enabled">Indicates whether the user account is enabled. Defaults to true.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public sealed record KeycloakUserConfig(
    string Username,
    string Email,
    string? TemporaryPassword = null,
    IReadOnlyList<string>? RealmRoles = null,
    bool Enabled = true,
    string? FirstName = null,
    string? LastName = null);
