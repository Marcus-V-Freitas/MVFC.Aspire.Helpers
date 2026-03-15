namespace MVFC.Aspire.Helpers.Tests.Unit.Keycloak.Helpers;

internal sealed class StubRealmSeed(string realmName) : IKeycloakRealmSeed
{
    public string RealmName => realmName;
}
