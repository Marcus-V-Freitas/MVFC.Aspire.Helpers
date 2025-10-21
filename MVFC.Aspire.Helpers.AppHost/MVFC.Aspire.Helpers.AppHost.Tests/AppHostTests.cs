namespace MVFC.Aspire.Helpers.AppHost.Tests;

public sealed class AppHostTests {
    [Fact]
    public async Task MongoOkStatusCode() {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Aspire_Helpers_AppHost_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/mongo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CloudStorageOkStatusCode() {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Aspire_Helpers_AppHost_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/bucket/bucket-teste");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PubSubOkStatusCode() {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Aspire_Helpers_AppHost_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api-exemplo");
        var response = await httpClient.GetAsync("/api/pub-sub-enter");

        await Task.Delay(TimeSpan.FromSeconds(5));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}