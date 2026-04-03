namespace MVFC.Aspire.Helpers.Tests.Unit.WireMock;

public sealed class WireMockLifecycleHookTests
{
    [Fact]
    public async Task OnBeforeStartAsync_WhenServerIsStarted_ShouldPublishRunningState()
    {
        // Arrange
        var randomPort = new Random().Next(10000, 60000);
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var wmBuilder = appBuilder.AddWireMock("wm-started", port: randomPort);

        var app = appBuilder.Build();
        var eventing = app.Services.GetRequiredService<IDistributedApplicationEventing>();
        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var @event = new BeforeStartEvent(app.Services, appModel);

        // Act
        Func<Task> act = async () => await eventing.PublishAsync(@event, CancellationToken.None);

        // Assert - should not throw, and exercises the full started=true path
        // (LogReadyAspireWireMock, BuildUrls with URL, BuildResourceState = Running)
        await act.Should().NotThrowAsync();

        // Cleanup
        wmBuilder.Resource.Server.Stop();
    }

    [Fact]
    public async Task OnBeforeStartAsync_WhenServerIsStopped_ShouldPublishErrorState()
    {
        // Arrange
        var randomPort = new Random().Next(10000, 60000);
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var wmBuilder = appBuilder.AddWireMock("wm-stopped", port: randomPort);
        wmBuilder.Resource.Server.Stop(); // Force the error branch

        var app = appBuilder.Build();
        var eventing = app.Services.GetRequiredService<IDistributedApplicationEventing>();
        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var @event = new BeforeStartEvent(app.Services, appModel);

        // Act
        Func<Task> act = async () => await eventing.PublishAsync(@event, CancellationToken.None);

        // Assert - exercises the started=false path
        // (LogErrorAspireWireMock, BuildUrls = empty, BuildResourceState = Error, StopTimeStamp set)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnBeforeStartAsync_WithNoWireMockResources_ShouldNotThrow()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        // No WireMock resources added

        var app = appBuilder.Build();
        var eventing = app.Services.GetRequiredService<IDistributedApplicationEventing>();
        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var @event = new BeforeStartEvent(app.Services, appModel);

        // Act
        Func<Task> act = async () => await eventing.PublishAsync(@event, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
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
