namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class FunctionalHttpResilienceTests
{
    [Theory]
    [InlineData("/api/pub-sub-enter")]
    [InlineData("/api/pub-sub-enter/")]
    [InlineData("/API/PUB-SUB-ENTER?source=test")]
    [InlineData("/api/%70ub-sub-enter")]
    [InlineData("/api/rabbitmq/consume/test-queue")]
    [InlineData("/api/redis/set/key/value")]
    [InlineData("/api/bucket/test-bucket")]
    public async Task FunctionalClient_WhenGetHasEffects_ShouldNotRepeatAfterServerFailure(string path)
    {
        // Arrange
        var effects = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() =>
            new TestHttpMessageHandler(_ =>
            {
                effects++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Act
        using var response = await client.GetAsync($"http://localhost{path}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        effects.Should().Be(1);
    }

    [Theory]
    [InlineData("/api/pub-sub-enter")]
    [InlineData("/api/rabbitmq/consume/test-queue")]
    [InlineData("/api/redis/set/key/value")]
    [InlineData("/api/bucket/test-bucket")]
    public async Task FunctionalClient_WhenResponseIsLostAfterEffect_ShouldPreserveFailureWithoutRepeating(string path)
    {
        // Arrange
        var effects = 0;
        var failure = new HttpRequestException("Response lost after applying the operation.");
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() =>
            new TestHttpMessageHandler(_ =>
            {
                effects++;
                return Task.FromException<HttpResponseMessage>(failure);
            }));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Act
        var act = () => client.GetAsync($"http://localhost{path}", TestContext.Current.CancellationToken);

        // Assert
        (await act.Should().ThrowAsync<HttpRequestException>()).Which.Should().BeSameAs(failure);
        effects.Should().Be(1);
    }

    [Theory]
    [InlineData("/api/redis/get/key")]
    [InlineData("/api/pub-sub-enter-info")]
    public async Task FunctionalClient_WhenGetIsReadOnly_ShouldRetainTransientRetry(string path)
    {
        // Arrange
        var calls = 0;
        var services = new ServiceCollection();
        FunctionalHttpResilience.AddClient(services).ConfigurePrimaryHttpMessageHandler(() =>
            new TestHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(
                ++calls == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK))));
        using var provider = services.BuildServiceProvider();
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        // Act
        using var response = await client.GetAsync($"http://localhost{path}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        calls.Should().Be(2);
    }

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
