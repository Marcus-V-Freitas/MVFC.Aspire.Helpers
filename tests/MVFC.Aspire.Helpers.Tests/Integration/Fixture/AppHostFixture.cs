namespace MVFC.Aspire.Helpers.Tests.Integration.Fixture;

public sealed class AppHostFixture : IAsyncLifetime 
{
    private ProjectAppHost _appHost = default!;
    private HttpClient _keycloakHttpClient = default!;
    private HttpClient _wireMockHttpClient = default!;
    private HttpClient _appHttpClient = default!;
    private HttpClient _apigeeHttpClient = default!;

    internal IPlaygroundApiClient PlaygroundApi { get; private set; } = default!;
    internal IWireMockApiClient WireMockApi { get; private set; } = default!;
    internal IKeycloakTokenClient KeycloakApi { get; private set; } = default!;
    internal IApigeeApiClient ApigeeApi { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _appHost = new ProjectAppHost();

        await _appHost.StartAsync().ConfigureAwait(false);

        _keycloakHttpClient = _appHost.CreateHttpClient("keycloak");
        _appHttpClient = _appHost.CreateHttpClient("api-exemplo");
        _wireMockHttpClient = await HttpTestExtensions.CreateHttpClient(_appHost, "wireMock").ConfigureAwait(false);
        _apigeeHttpClient = _appHost.CreateHttpClient("apigee-emulator", "http");
        PlaygroundApi = RestService.For<IPlaygroundApiClient>(_appHttpClient);
        WireMockApi = RestService.For<IWireMockApiClient>(_wireMockHttpClient);
        KeycloakApi = RestService.For<IKeycloakTokenClient>(_keycloakHttpClient);
        ApigeeApi = RestService.For<IApigeeApiClient>(_apigeeHttpClient);
    }

    public async ValueTask DisposeAsync()
    {
        _keycloakHttpClient?.Dispose();
        _appHttpClient?.Dispose();
        _wireMockHttpClient?.Dispose();
        _apigeeHttpClient?.Dispose();
        await _appHost.DisposeAsync().ConfigureAwait(false);
    }
}
