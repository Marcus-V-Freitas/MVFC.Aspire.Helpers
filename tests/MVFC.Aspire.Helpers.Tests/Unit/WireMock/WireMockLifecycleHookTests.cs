namespace MVFC.Aspire.Helpers.Tests.Unit.WireMock;

public sealed class WireMockLifecycleHookTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OnBeforeStartAsync_ShouldPublishStateBeforeReadiness(bool started)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var wireMock = builder.AddWireMock("wiremock", port: null);
        using var server = wireMock.Resource.Server;

        if (!started)
            server.Stop();

        using var app = builder.Build();
        var eventing = app.Services.GetRequiredService<IDistributedApplicationEventing>();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var notifications = app.Services.GetRequiredService<ResourceNotificationService>();
        var hook = new WireMockLifecycleHook(
            app.Services.GetRequiredService<ResourceNotificationService>(), eventing,
            app.Services.GetRequiredService<ResourceLoggerService>(), app.Services);
        await hook.SubscribeAsync(eventing, builder.ExecutionContext, TestContext.Current.CancellationToken);
        var readyCount = 0;
        eventing.Subscribe<ResourceReadyEvent>((@event, _) =>
        {
            if (@event.Resource == wireMock.Resource)
            {
                notifications.TryGetCurrentState(wireMock.Resource.Name, out var current).Should().BeTrue();
                current!.Snapshot.State!.Text.Should().Be("Running");
                readyCount++;
            }

            return Task.CompletedTask;
        });

        // Act
        await eventing.PublishAsync(new BeforeStartEvent(app.Services, model), TestContext.Current.CancellationToken);

        // Assert
        readyCount.Should().Be(started ? 1 : 0);
        notifications.TryGetCurrentState(wireMock.Resource.Name, out var state).Should().BeTrue();
        state!.Snapshot.State!.Text.Should().Be(started ? "Running" : "Error");
        state.Snapshot.Urls.Should().HaveCount(started ? 1 : 0);
        state.Snapshot.StartTimeStamp.HasValue.Should().Be(started);
        state.Snapshot.StopTimeStamp.HasValue.Should().Be(!started);
    }

    [Fact]
    public async Task OnBeforeStartAsync_WithNoWireMockResources_ShouldNotPublishReadiness()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        using var app = builder.Build();
        var eventing = app.Services.GetRequiredService<IDistributedApplicationEventing>();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hook = new WireMockLifecycleHook(
            app.Services.GetRequiredService<ResourceNotificationService>(), eventing,
            app.Services.GetRequiredService<ResourceLoggerService>(), app.Services);
        await hook.SubscribeAsync(eventing, builder.ExecutionContext, TestContext.Current.CancellationToken);
        var readyCount = 0;
        eventing.Subscribe<ResourceReadyEvent>((_, _) =>
        {
            readyCount++;
            return Task.CompletedTask;
        });

        // Act
        await eventing.PublishAsync(new BeforeStartEvent(app.Services, model), TestContext.Current.CancellationToken);

        // Assert
        readyCount.Should().Be(0);
    }
    [Fact]
    public void LogExtensions_LogStartingAspireWireMock_ShouldNotThrow()
    {
        // Arrange
        var logger = NullLoggerFactory.Instance.CreateLogger("WireMockTest");

        // Act
        var act = () => logger.LogStartingAspireWireMock();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogExtensions_LogReadyAspireWireMock_ShouldNotThrow()
    {
        // Arrange
        var logger = NullLoggerFactory.Instance.CreateLogger("WireMockTest");

        // Act
        var act = () => logger.LogReadyAspireWireMock("wiremock-test", 8080);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogExtensions_LogErrorAspireWireMock_ShouldNotThrow()
    {
        // Arrange
        var logger = NullLoggerFactory.Instance.CreateLogger("WireMockTest");

        // Act
        var act = () => logger.LogErrorAspireWireMock("wiremock-test", 8080);

        // Assert
        act.Should().NotThrow();
    }
}
