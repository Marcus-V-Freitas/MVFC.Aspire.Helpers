namespace MVFC.Aspire.Helpers.Keycloak.Resources;

public sealed class KeycloakResource(string name)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const string HTTP_ENDPOINT_NAME = "http";

    internal string AdminUsername { get; set; } = KeycloakDefaults.DEFAULT_ADMIN_USER;

    internal string AdminPassword { get; set; } = KeycloakDefaults.DEFAULT_ADMIN_PASS;

    internal IReadOnlyCollection<IKeycloakRealmSeed>? Seeds { get; set; }

    private EndpointReference? _endpointRef;

    private EndpointReference HttpEndpoint =>
        _endpointRef ??= new EndpointReference(this, HTTP_ENDPOINT_NAME);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Scheme}://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}");
}
