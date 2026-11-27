namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoDumpsExecutionTests
{
    [Fact]
    public async Task ExecuteAsync_WhenConsumersOverlap_ShouldShareOneExecution()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>()).Returns(completion.Task);
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        var token = TestContext.Current.CancellationToken;

        // Act
        var first = execution.ExecuteAsync(mongo.Resource, token);
        var second = execution.ExecuteAsync(mongo.Resource, token);
        var pending = !first.IsCompleted;
        completion.SetResult();
        await Task.WhenAll(first, second);
        var subsequent = execution.ExecuteAsync(mongo.Resource, token);

        // Assert
        pending.Should().BeTrue();
        second.Should().BeSameAs(first);
        subsequent.Should().BeSameAs(first);
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDumpFails_ShouldShareFailureWithoutReplayingWrites()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        var failure = new InvalidOperationException("Dump failed.");
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(failure));
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        var token = TestContext.Current.CancellationToken;

        // Act
        var first = execution.ExecuteAsync(mongo.Resource, token);
        var second = execution.ExecuteAsync(mongo.Resource, token);
        var act = () => first;

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
        second.Should().BeSameAs(first);
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), token);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCanceledBeforeStarting_ShouldNotExecuteDumps()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var act = () => execution.ExecuteAsync(mongo.Resource, cancellation.Token);

        // Assert
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        await dump.DidNotReceive().ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCanceledDuringDump_ShouldPreserveCancellationForEveryConsumer()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        using var cancellation = new CancellationTokenSource();
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), cancellation.Token).Returns(_ =>
        {
            cancellation.Cancel();
            return Task.FromCanceled(cancellation.Token);
        });
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();

        // Act
        var first = execution.ExecuteAsync(mongo.Resource, cancellation.Token);
        var second = execution.ExecuteAsync(mongo.Resource, TestContext.Current.CancellationToken);
        var act = () => first;

        // Assert
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        second.Should().BeSameAs(first);
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), cancellation.Token);
    }

    [Fact]
    public async Task WaitAsync_ShouldRequirePrimaryAndDumpCompletion()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        var primaryReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dumpStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dumpFinished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            dumpStarted.SetResult();
            return dumpFinished.Task;
        });
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        var token = TestContext.Current.CancellationToken;

        // Act
        var starting = execution.WaitAsync(mongo.Resource, primaryReady.Task, token, token);
        var startedBeforePrimary = dumpStarted.Task.IsCompleted;
        primaryReady.SetResult();
        await dumpStarted.Task.WaitAsync(token);
        var startedBeforeDumpsFinished = starting.IsCompleted;
        dumpFinished.SetResult();
        await starting;

        // Assert
        startedBeforePrimary.Should().BeFalse();
        startedBeforeDumpsFinished.Should().BeFalse();
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), token);
    }

    [Fact]
    public async Task WaitAsync_WhenReadinessFails_ShouldNotExecuteDumps()
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        var failure = new InvalidOperationException("Primary unavailable.");
        var token = TestContext.Current.CancellationToken;

        // Act
        var act = () => execution.WaitAsync(mongo.Resource, Task.FromException(failure), token, token);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
        await dump.DidNotReceive().ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WaitAsync_WhenConsumerCancels_ShouldNotCancelSharedDump(bool cancelBeforeStarting)
    {
        // Arrange
        var mongo = CreateMongo();
        var dump = Substitute.For<IMongoClassDump>();
        var dumpFinished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>()).Returns(dumpFinished.Task);
        mongo.WithDumps(dump);
        var execution = new MongoDumpsExecutedAnnotation();
        using var consumerCancellation = new CancellationTokenSource();
        var token = TestContext.Current.CancellationToken;

        if (cancelBeforeStarting)
            consumerCancellation.Cancel();

        // Act
        var first = execution.WaitAsync(mongo.Resource, Task.CompletedTask, token, consumerCancellation.Token);
        consumerCancellation.Cancel();
        var canceledConsumer = () => first;
        var second = execution.WaitAsync(mongo.Resource, Task.CompletedTask, token, token);
        dumpFinished.SetResult();
        await second;

        // Assert
        (await canceledConsumer.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(consumerCancellation.Token);
        await dump.Received(1).ExecuteDumpAsync(Arg.Any<IMongoClient>(), token);
    }

    private static IResourceBuilder<MongoReplicaSetResource> CreateMongo()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        var mongo = builder.AddMongoReplicaSet("mongo");
        var endpoint = mongo.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 27017);

        return mongo;
    }
}
