namespace MVFC.Aspire.Helpers.Keycloak.Builders;

internal static class KeycloakRealmJsonBuilder
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    internal static string Build(IKeycloakRealmSeed seed)
    {
        var realm = new Dictionary<string, object?>
        {
            ["realm"] = seed.RealmName,
            ["enabled"] = seed.Enabled,
        };

        if (seed.Roles.Count > 0)
            realm["roles"] = BuildRoles(seed.Roles);

        if (seed.Clients.Count > 0)
            realm["clients"] = BuildClients(seed.Clients);

        if (seed.Groups.Count > 0)
            realm["groups"] = BuildGroups(seed.Groups);

        if (seed.Users.Count > 0)
            realm["users"] = BuildUsers(seed.Users);

        return JsonSerializer.Serialize(realm, JsonOptions);
    }

    private static object BuildRoles(IReadOnlyList<KeycloakRoleConfig> roles) =>
        new { realm = roles.Select(r => new { name = r.Name, description = r.Description }) };

    private static IEnumerable<object> BuildClients(IReadOnlyList<KeycloakClientConfig> clients) =>
        clients.Select(c => new
        {
            clientId = c.ClientId,
            secret = c.ClientSecret,
            publicClient = c.PublicClient,
            serviceAccountsEnabled = c.ServiceAccountsEnabled,
            directAccessGrantsEnabled = c.DirectAccessGrantsEnabled,
            redirectUris = c.RedirectUris,
            webOrigins = c.WebOrigins,
            protocol = "openid-connect",
            enabled = true,
            protocolMappers = c.AddAudienceMapper ? BuildAudienceMapper(c.ClientId) : null,
        });

    private static IEnumerable<object> BuildGroups(IReadOnlyList<KeycloakGroupConfig> groups) =>
        groups.Select(g => new { name = g.Name, realmRoles = g.RealmRoles });

    private static IEnumerable<object> BuildUsers(IReadOnlyList<KeycloakUserConfig> users) =>
        users.Select(u => new
        {
            username = u.Username,
            email = u.Email,
            firstName = u.FirstName,
            lastName = u.LastName,
            enabled = u.Enabled,
            realmRoles = u.RealmRoles,
            credentials = BuildCredentials(u.TemporaryPassword),
        });

    private static object[]? BuildCredentials(string? temporaryPassword) =>
        temporaryPassword is null
            ? null
            : [new { type = "password", value = temporaryPassword, temporary = true }];

    private static object[] BuildAudienceMapper(string clientId) =>
    [
        new
        {
            name            = "audience-mapper",
            protocol        = "openid-connect",
            protocolMapper  = "oidc-audience-mapper",
            consentRequired = false,
            config          = new Dictionary<string, string>
            {
                ["included.client.audience"] = clientId,
                ["id.token.claim"]           = "false",
                ["access.token.claim"]       = "true",
            },
        }
    ];
}
