namespace MVFC.Aspire.Helpers.Keycloak.Seeds;

/// <summary>
/// Interface for defining a Keycloak Realm seed that can be used to populate the server with initial data.
/// </summary>
public interface IKeycloakRealmSeed
{
    /// <summary>
    /// Gets the name of the Keycloak realm to be created or imported.
    /// </summary>
    public string RealmName { get; }

    /// <summary>
    /// Gets a value indicating whether the realm should be enabled upon creation. Defaults to true.
    /// </summary>
    public bool Enabled => true;

    /// <summary>
    /// Gets the list of realm roles to be created.
    /// </summary>
    public IReadOnlyList<KeycloakRoleConfig> Roles => [];

    /// <summary>
    /// Gets the list of OAuth/OpenID Connect clients to be configured in the realm.
    /// </summary>
    public IReadOnlyList<KeycloakClientConfig> Clients => [];

    /// <summary>
    /// Gets the list of users to be provisioned in the realm.
    /// </summary>
    public IReadOnlyList<KeycloakUserConfig> Users => [];

    /// <summary>
    /// Gets the list of groups to be created in the realm.
    /// </summary>
    public IReadOnlyList<KeycloakGroupConfig> Groups => [];
}
