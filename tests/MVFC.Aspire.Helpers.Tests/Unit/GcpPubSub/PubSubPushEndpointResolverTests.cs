namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubPushEndpointResolverTests
{
    [Fact]
    public async Task ResolveAsync_ShouldUseMessageDestination()
    {
        // Arrange
        var destination = CreateEndpoint("destination", 5050);

        var message = new MessageConfig("topic", "sub", "/callback") { PushEndpointReference = destination };

        // Act
        var address = await PubSubPushEndpointResolver.ResolveAsync(message, TestContext.Current.CancellationToken);

        // Assert
        address.Should().Be($"http://{PubSubDefaults.DockerInternalHost}:5050");
        message.PushEndpointReference.Should().BeSameAs(destination);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ResolveAsync_WhenPullOnly_ShouldNotWaitForDestination(string? route)
    {
        // Arrange
        var destination = CreateEndpoint("destination");
        var message = new MessageConfig("topic", "sub", route) { PushEndpointReference = destination };

        // Act
        var address = await PubSubPushEndpointResolver.ResolveAsync(message, TestContext.Current.CancellationToken);

        // Assert
        address.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveAsync_WhenPushHasNoDestination_ShouldExplainConfiguration()
    {
        // Arrange
        var message = new MessageConfig("topic", "sub", "/callback");

        // Act
        var act = async () => await PubSubPushEndpointResolver.ResolveAsync(message, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PushEndpointReference*");
    }

    [Fact]
    public async Task ResolveAsync_ShouldWaitForAllocationBeforeReadingPort()
    {
        // Arrange
        var destination = CreateEndpoint("destination");
        var message = new MessageConfig("topic", "sub", "/callback") { PushEndpointReference = destination };

        // Act
        var task = PubSubPushEndpointResolver.ResolveAsync(message, TestContext.Current.CancellationToken).AsTask();
        var wasWaiting = !task.IsCompleted;
        var annotation = destination.Resource.Annotations.OfType<EndpointAnnotation>().Single();

        annotation.AllocatedEndpoint = new AllocatedEndpoint(annotation, "localhost", 5050);

        var address = await task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        wasWaiting.Should().BeTrue();
        address.Should().Be($"http://{PubSubDefaults.DockerInternalHost}:5050");
    }

    [Fact]
    public async Task ResolveAsync_WhenCallerCancels_ShouldStopWaitingForAllocation()
    {
        // Arrange
        var destination = CreateEndpoint("destination");
        var message = new MessageConfig("topic", "sub", "/callback") { PushEndpointReference = destination };
        using var cancellation = new CancellationTokenSource();

        // Act
        var task = PubSubPushEndpointResolver.ResolveAsync(message, cancellation.Token).AsTask();

        cancellation.Cancel();

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static EndpointReference CreateEndpoint(string name, int? port = null)
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = builder.AddContainer(name, "unused").WithHttpEndpoint(targetPort: 80);
        var annotation = resource.Resource.Annotations.OfType<EndpointAnnotation>().Single();

        if (port.HasValue)
            annotation.AllocatedEndpoint = new AllocatedEndpoint(annotation, "localhost", port.Value);

        return resource.GetEndpoint("http");
    }
}
