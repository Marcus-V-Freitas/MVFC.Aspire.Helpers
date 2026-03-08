namespace MVFC.Aspire.Helpers.Keycloak.Configs;

/// <summary>
/// Configuration for a Keycloak Client (OAuth/OpenID Connect application).
/// </summary>
/// <param name="ClientId">The unique identifier for the client (e.g., "my-api").</param>
/// <param name="ClientSecret">The secret used for confidential clients to authenticate with Keycloak.</param>
/// <param name="PublicClient">Indicates if this is a public client (typically frontend apps without a secret).</param>
/// <param name="ServiceAccountsEnabled">Enables service accounts (Client Credentials Grant) for this client.</param>
/// <param name="DirectAccessGrantsEnabled">Enables direct access grants (Resource Owner Password Credentials Grant).</param>
/// <param name="AddAudienceMapper">Automatically adds an audience mapper so tokens issued to this client contain its intended audience.</param>
/// <param name="RedirectUris">A list of valid redirect URIs for this client.</param>
/// <param name="WebOrigins">A list of valid CORS web origins for this client.</param>
public sealed record KeycloakClientConfig(
    string ClientId,
    string? ClientSecret = null,
    bool PublicClient = false,
    bool ServiceAccountsEnabled = false,
    bool DirectAccessGrantsEnabled = false,
    bool AddAudienceMapper = true,
    IReadOnlyList<string>? RedirectUris = null,
    IReadOnlyList<string>? WebOrigins = null);
