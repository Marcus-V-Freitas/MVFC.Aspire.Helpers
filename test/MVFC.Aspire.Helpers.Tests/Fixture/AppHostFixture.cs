namespace MVFC.Aspire.Helpers.Tests.Fixture;

public sealed class AppHostFixture : IAsyncLifetime 
{
    private ProjectAppHost _appHost = default!;
    private HttpClient _keycloakHttpClient = default!;
    private HttpClient _wireMockHttpClient = default!;
    private HttpClient _appHttpClient = default!;

    internal IPlaygroundApiClient PlaygroundApi { get; private set; } = default!;
    internal IWireMockApiClient WireMockApi { get; private set; } = default!;
    internal IKeycloakTokenClient KeycloakApi { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _appHost = new ProjectAppHost();

        await _appHost.StartAsync().ConfigureAwait(false);

        _keycloakHttpClient = _appHost.CreateHttpClient("keycloak");
        _appHttpClient = _appHost.CreateHttpClient("api-exemplo");
        _wireMockHttpClient = await HttpTestExtensions.CreateHttpClient(_appHost, "wireMock").ConfigureAwait(false);

        PlaygroundApi = RestService.For<IPlaygroundApiClient>(_appHttpClient);
        WireMockApi = RestService.For<IWireMockApiClient>(_wireMockHttpClient);
        KeycloakApi = RestService.For<IKeycloakTokenClient>(_keycloakHttpClient);
    }

    public async ValueTask DisposeAsync()
    {
        _keycloakHttpClient?.Dispose();
        _appHttpClient?.Dispose();
        _wireMockHttpClient?.Dispose();
        await _appHost.DisposeAsync().ConfigureAwait(false);
    }
}
