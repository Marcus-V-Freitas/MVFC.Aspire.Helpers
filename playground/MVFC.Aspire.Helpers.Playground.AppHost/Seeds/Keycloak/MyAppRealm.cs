namespace MVFC.Aspire.Helpers.Playground.AppHost.Seeds.Keycloak;

internal sealed class MyAppRealm : IKeycloakRealmSeed
{
    public string RealmName => "my-app";

    public IReadOnlyList<KeycloakRoleConfig> Roles =>
    [
        new("admin", "Acesso total"),
        new("user",  "Acesso básico"),
    ];

    public IReadOnlyList<KeycloakClientConfig> Clients =>
    [
        new(ClientId: "my-api",
            ClientSecret: "api-secret-1234",
            ServiceAccountsEnabled: true,
            DirectAccessGrantsEnabled: true,
            AddAudienceMapper: true),
    ];

    public IReadOnlyList<KeycloakGroupConfig> Groups =>
    [
        new(Name: "Admins", RealmRoles: ["admin"]),
        new(Name: "Users",  RealmRoles: ["user"]),
    ];

    public IReadOnlyList<KeycloakUserConfig> Users =>
    [
        new(Username: "marcus.admin",
            Email: "marcus@myapp.com",
            TemporaryPassword: "Admin@123",
            RealmRoles: ["admin"],
            FirstName: "Marcus",
            LastName: "Admin"),
        new(Username: "john.user",
            Email: "john@myapp.com",
            TemporaryPassword: "User@123",
            RealmRoles: ["user"],
            FirstName: "John",
            LastName: "User"),
    ];
}
