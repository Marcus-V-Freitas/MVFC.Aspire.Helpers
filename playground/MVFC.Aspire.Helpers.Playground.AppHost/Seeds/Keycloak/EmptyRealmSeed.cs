namespace MVFC.Aspire.Helpers.Playground.AppHost.Seeds.Keycloak;

internal sealed record EmptyRealmSeed : IKeycloakRealmSeed
{
    public string RealmName => "empty-realm";
}
