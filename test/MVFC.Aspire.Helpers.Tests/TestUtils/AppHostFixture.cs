namespace MVFC.Aspire.Helpers.Tests.TestUtils;

public sealed class AppHostFixture : IAsyncLifetime {

    private ProjectAppHost _appHost = default!;

    public HttpClient HttpClient { get; private set; } = default!;
    public HttpClient AppHttpClient { get; private set; } = default!;

    public async ValueTask InitializeAsync() {
        _appHost = new ProjectAppHost();

        await _appHost.StartAsync();

        AppHttpClient = _appHost.CreateHttpClient("api-exemplo");
        HttpClient = await _appHost.CreateWireMockHttpClient("wireMock");
    }

    public async ValueTask DisposeAsync() {
        AppHttpClient?.Dispose();
        HttpClient?.Dispose();
        await _appHost.DisposeAsync();
    }
}