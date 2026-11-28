namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubEmulatorInitializerTests
{
    [Fact]
    public async Task InitializeAsync_WhenConfigIsInvalid_ShouldFailBeforeWaitingForEmulatorEndpoint()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var pubSub = builder.AddGcpPubSub("pubsub")
            .WithPubSubConfigs(new PubSubConfig("project", new MessageConfig("topic", "sub", "/callback")));
        var time = Substitute.For<TimeProvider>();

        // Act
        var act = () => PubSubEmulatorInitializer.InitializeAsync(pubSub.Resource, NullLogger.Instance,
            TestContext.Current.CancellationToken, time);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*sub*PushEndpointReference*");
        time.DidNotReceive().CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>());
        pubSub.Resource.HttpEndpoint.IsAllocated.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, 60)]
    [InlineData(true, 60)]
    [InlineData(false, 7)]
    [InlineData(true, 7)]
    public async Task InitializeAsync_WhenEndpointIsNotAllocated_ShouldHonorSharedDeadline(bool emulatorAllocated, int timeoutSeconds)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var consumer = builder.AddTestProject("consumer");
        var pubSub = builder.AddGcpPubSub("pubsub")
            .WithConfigurationTimeout(timeoutSeconds)
            .WithPubSubConfigs(new PubSubConfig("project", new MessageConfig("topic", "sub", "/callback")
            {
                PushEndpointReference = consumer.GetEndpoint("http"),
            }));

        consumer.WithReference(pubSub);

        if (emulatorAllocated)
        {
            var endpoint = pubSub.Resource.Annotations.OfType<EndpointAnnotation>().Single(e => e.Name == "http");
            endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 8681);
        }

        TimerCallback? callback = null;
        object? state = null;
        var time = Substitute.For<TimeProvider>();

        time.CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>())
            .Returns(call =>
            {
                callback = call.ArgAt<TimerCallback>(0);
                state = call.ArgAt<object?>(1);

                return Substitute.For<ITimer>();
            });

        // Act
        var task = PubSubEmulatorInitializer.InitializeAsync(pubSub.Resource, NullLogger.Instance, CancellationToken.None, time);

        var wasWaiting = !task.IsCompleted;
        callback!(state);

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        wasWaiting.Should().BeTrue();
        await act.Should().ThrowAsync<TimeoutException>().WithMessage($"*{timeoutSeconds} seconds*");

        time.Received(1).CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(),
            TimeSpan.FromSeconds(timeoutSeconds), Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public async Task InitializeAsync_WhenCallerCancelsEndpointResolution_ShouldPreserveCancellation()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var pubSub = builder.AddGcpPubSub("pubsub")
            .WithPubSubConfigs(new PubSubConfig("project", new MessageConfig("topic", "sub")));
        using var cancellation = new CancellationTokenSource();

        // Act
        var task = PubSubEmulatorInitializer.InitializeAsync(pubSub.Resource, NullLogger.Instance, cancellation.Token);

        cancellation.Cancel();

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        var exception = (await act.Should().ThrowAsync<OperationCanceledException>()).Which;

        exception.CancellationToken.Should().Be(cancellation.Token);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoSubscriptions_ShouldNotResolveEndpoints()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var pubSub = builder.AddGcpPubSub("pubsub")
            .WithPubSubConfigs(new PubSubConfig("project", new MessageConfig("topic")));
        var time = Substitute.For<TimeProvider>();

        // Act
        await PubSubEmulatorInitializer.InitializeAsync(pubSub.Resource, NullLogger.Instance, TestContext.Current.CancellationToken, time);

        // Assert
        time.DidNotReceive().CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>());
    }
}
