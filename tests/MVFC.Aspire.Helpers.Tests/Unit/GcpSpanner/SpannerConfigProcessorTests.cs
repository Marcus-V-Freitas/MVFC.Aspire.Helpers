namespace MVFC.Aspire.Helpers.Tests.Unit.GcpSpanner;

public sealed class SpannerConfigProcessorTests
{
    [Fact]
    public async Task ConfigureAsync_WhenResourcesExist_ShouldNotCreateThem()
    {
        // Arrange
        var instances = Substitute.For<InstanceAdminClient>();
        var databases = Substitute.For<DatabaseAdminClient>();
        var config = new SpannerConfig("project", "instance", "database");

        // Act
        await SpannerConfigProcessor.ConfigureAsync([config], instances, databases, TestContext.Current.CancellationToken);

        // Assert
        await instances.DidNotReceive().CreateInstanceAsync(Arg.Any<CreateInstanceRequest>(), Arg.Any<CancellationToken>());
        await databases.DidNotReceive().CreateDatabaseAsync(Arg.Any<CreateDatabaseRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfigureAsync_WhenMissing_ShouldObserveCompletedOperation(bool failed)
    {
        // Arrange
        var instances = Substitute.For<InstanceAdminClient>();
        var databases = Substitute.For<DatabaseAdminClient>();
        var config = new SpannerConfig("project", "instance", "database", ["CREATE TABLE Example (Id INT64) PRIMARY KEY (Id)"]);
        instances.GetInstanceAsync(Arg.Any<InstanceName>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SpannerInstance>(new RpcException(new Status(StatusCode.NotFound, "missing"))));
        databases.GetDatabaseAsync(Arg.Any<DatabaseName>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SpannerDatabase>(new RpcException(new Status(StatusCode.NotFound, "missing"))));
        var raw = new Google.LongRunning.Operation { Name = "operation", Done = true };

        if (failed)
            raw.Error = new Google.Rpc.Status { Code = (int)StatusCode.PermissionDenied, Message = "provisioning denied" };

        instances.CreateInstanceAsync(Arg.Any<CreateInstanceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Google.LongRunning.Operation<SpannerInstance, CreateInstanceMetadata>(raw, Substitute.For<Google.LongRunning.OperationsClient>()));
        databases.CreateDatabaseAsync(Arg.Any<CreateDatabaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Google.LongRunning.Operation<SpannerDatabase, CreateDatabaseMetadata>(
                new Google.LongRunning.Operation { Name = "database", Done = true }, Substitute.For<Google.LongRunning.OperationsClient>()));

        // Act
        var act = () => SpannerConfigProcessor.ConfigureAsync([config], instances, databases, TestContext.Current.CancellationToken);

        // Assert
        if (failed)
        {
            await act.Should().ThrowAsync<Google.LongRunning.OperationFailedException>();
            await databases.DidNotReceive().GetDatabaseAsync(Arg.Any<DatabaseName>(), Arg.Any<CancellationToken>());
        }
        else
        {
            await act.Should().NotThrowAsync();
            await databases.Received(1).CreateDatabaseAsync(
                Arg.Is<CreateDatabaseRequest>(r => r.CreateStatement == "CREATE DATABASE `database`" && r.ExtraStatements.SequenceEqual(config.DdlStatements)),
                TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task ConfigureAsync_WhenPolling_ShouldForwardCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var instances = Substitute.For<InstanceAdminClient>();
        var databases = Substitute.For<DatabaseAdminClient>();
        var operations = Substitute.For<Google.LongRunning.OperationsClient>();
        var pollStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        instances.GetInstanceAsync(Arg.Any<InstanceName>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SpannerInstance>(new RpcException(new Status(StatusCode.NotFound, "missing"))));
        instances.CreateInstanceAsync(Arg.Any<CreateInstanceRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Google.LongRunning.Operation<SpannerInstance, CreateInstanceMetadata>(
                new Google.LongRunning.Operation { Name = "pending" }, operations));
        operations.GetOperationAsync(Arg.Any<string>(), Arg.Any<CallSettings>())
            .Returns(async call =>
            {
                var token = call.Arg<CallSettings>().CancellationToken!.Value;
                pollStarted.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return new Google.LongRunning.Operation();
            });

        // Act
        var provisioning = SpannerConfigProcessor.ConfigureAsync([new("project", "instance", "database")], instances, databases, cts.Token);
        await pollStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cts.CancelAsync();
        var act = () => provisioning.WaitAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        await databases.DidNotReceive().GetDatabaseAsync(Arg.Any<DatabaseName>(), Arg.Any<CancellationToken>());
    }
}
