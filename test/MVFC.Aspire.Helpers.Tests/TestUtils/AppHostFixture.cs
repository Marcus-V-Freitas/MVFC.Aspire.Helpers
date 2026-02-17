namespace MVFC.Aspire.Helpers.Tests.TestUtils;

public sealed class AppHostFixture : IAsyncLifetime {

    private IDistributedApplicationTestingBuilder _appHost = default!;
    private DistributedApplication _distributedApplication = default!;

    public HttpClient HttpClient { get; private set; } = default!;
    public HttpClient AppHttpClient { get; private set; } = default!;

    public async ValueTask InitializeAsync() {
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Aspire_Helpers_Playground_AppHost>();

        _distributedApplication = await _appHost.BuildAsync();
        await _distributedApplication.StartAsync();

        AppHttpClient = _distributedApplication.CreateHttpClient("api-exemplo");
        HttpClient = new HttpClient() {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    public async ValueTask DisposeAsync() {
        AppHttpClient?.Dispose();
        HttpClient?.Dispose();
        await _distributedApplication.DisposeAsync();
        await _appHost.DisposeAsync();
    }
}