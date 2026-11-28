namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoDumpsReadinessTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WaitForDumps_ShouldShareOneExecutionWithLegacyReadiness_RegardlessOfFluentOrder(bool waitFirst)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mongo = builder.AddMongoReplicaSet("mongo");
        var consumer = builder.AddTestProject("api");
        var endpoint = mongo.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 27017);
        var dump = Substitute.For<IMongoClassDump>();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>()).Returns(completion.Task);

        if (waitFirst)
            consumer.WaitForDumps(mongo);

        consumer.WithReference(mongo);
        mongo.WithDumps(dump);

        if (!waitFirst)
            consumer.WaitForDumps(mongo);

        consumer.WaitForDumps(mongo);
        using var services = new ServiceCollection().BuildServiceProvider();
        var execution = mongo.Resource.Annotations.OfType<MongoDumpsExecutedAnnotation>().Single();
        var token = TestContext.Current.CancellationToken;

        // Act
        var starting = execution.WaitAsync(mongo.Resource, Task.CompletedTask, token, token);
        var pending = !starting.IsCompleted;
        completion.SetResult();
        await starting;
        await builder.Eventing.PublishAsync(new ResourceReadyEvent(consumer.Resource, services), token);

        // Assert
        pending.Should().BeTrue();
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>());
        consumer.Resource.Annotations.OfType<MongoDumpsWaitAnnotation>().Should().ContainSingle();
        mongo.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().ContainSingle();
    }

    [Fact]
    public void WaitForDumps_WhenProjectIsNull_ShouldRejectConfiguration()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mongo = builder.AddMongoReplicaSet("mongo");
        IResourceBuilder<ProjectResource>? project = null;

        // Act
        var act = () => project!.WaitForDumps(mongo);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("project");
    }

    [Fact]
    public void WaitForDumps_WhenMongoIsNull_ShouldRejectConfiguration()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var consumer = builder.AddTestProject("api");

        // Act
        var act = () => consumer.WaitForDumps(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("mongo");
    }
}
