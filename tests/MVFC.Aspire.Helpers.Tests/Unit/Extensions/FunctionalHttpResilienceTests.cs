namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class FunctionalHttpResilienceTests
{
    [Theory]
    [InlineData("GET", 500, 2)]
    [InlineData("GET", 408, 2)]
    [InlineData("POST", 500, 1)]
    [InlineData("GET", 429, 1)]
    [InlineData("GET", 404, 1)]
    public async Task FunctionalClient_ShouldRetryOnlySafeTransientResponses(string method, int status, int attempts)
    {
        // Arrange
        var calls = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() =>
            new TestHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(
                ++calls == 1 ? (HttpStatusCode)status : HttpStatusCode.OK))));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/test");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        calls.Should().Be(attempts);
        response.StatusCode.Should().Be(attempts == 2 ? HttpStatusCode.OK : (HttpStatusCode)status);
    }

    [Fact]
    public async Task NamedProbe_ShouldNotInheritFunctionalRetries()
    {
        // Arrange
        var calls = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services);
        services.AddHttpClient("probe").ConfigurePrimaryHttpMessageHandler(() => new TestHttpMessageHandler(_ =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("probe");

        // Act
        using var response = await client.GetAsync("http://localhost/health", TestContext.Current.CancellationToken);

        // Assert
        calls.Should().Be(1);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task FunctionalClient_ShouldNotRetryPermanentExceptions()
    {
        // Arrange
        var calls = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() => new TestHttpMessageHandler(_ =>
        {
            calls++;
            throw new InvalidOperationException("Permanent failure");
        }));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Act
        var act = () => client.GetAsync("http://localhost/test", TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task FunctionalClient_WhenCanceled_ShouldStopWithoutRetry()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() => new TestHttpMessageHandler(ct =>
        {
            calls++;
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(ct);
        }));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Act
        var act = () => client.GetAsync("http://localhost/test", cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(1);
    }
}
