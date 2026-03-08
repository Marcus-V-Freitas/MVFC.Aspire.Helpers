namespace MVFC.Aspire.Helpers.Playground.AppHost.Seeds;

internal sealed record EmptyRealmSeed : IKeycloakRealmSeed
{
    public string RealmName => "empty-realm";
}
