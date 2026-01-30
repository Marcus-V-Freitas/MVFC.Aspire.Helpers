namespace MVFC.Aspire.Helpers.Tests.TestUtils;

public sealed class AppHostFixture : IAsyncLifetime {
    public IDistributedApplicationTestingBuilder AppHost { get; private set; } = default!;

    public DistributedApplication DistributedApplication { get; private set; } = default!;

    public HttpClient HttpClient { get; private set; } = default!;

    public async ValueTask InitializeAsync() {
        AppHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Aspire_Helpers_Playground_AppHost>();

        DistributedApplication = await AppHost.BuildAsync();
        await DistributedApplication.StartAsync();

        HttpClient = new HttpClient() {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    public async ValueTask DisposeAsync() {
        await DistributedApplication.DisposeAsync();
        await AppHost.DisposeAsync();
    }
}