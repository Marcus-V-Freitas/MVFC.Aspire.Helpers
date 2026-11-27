namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubConfigurationResilienceTests
{
    private static readonly PubSubConfig[] _configs =
        [new("project", new MessageConfig("topic", "sub", "/callback") { DeadLetterTopic = "dlq", PushEndpointReference = CreatePushEndpoint() })];

    [Fact]
    public async Task ConfigureAsync_WhenLaterConfigIsInvalid_ShouldNotReadOrUpdateAnySubscription()
    {
        // Arrange
        var reads = 0;
        var updates = 0;
        PubSubConfig[] configs =
        [
            new("project", new MessageConfig("topic", "valid")),
            new("project", new MessageConfig("topic", "invalid", "/callback")),
        ];
        var builder = CreateClientBuilder((request, _) =>
        {
            reads++;
            return Task.FromResult(new Subscription { Name = request.Subscription });
        }, (request, _) =>
        {
            updates++;
            return Task.FromResult(request.Subscription);
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(configs, builder, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*invalid*PushEndpointReference*");
        reads.Should().Be(0);
        updates.Should().Be(0);
    }

    [Fact]
    public async Task ConfigureAsync_WhenDuplicateConflicts_ShouldNotReadOrUpdateAnySubscription()
    {
        // Arrange
        var reads = 0;
        PubSubConfig[] configs =
        [
            new("project", new MessageConfig("topic", "sub") { AckDeadlineSeconds = 10 }),
            new("project", new MessageConfig("topic", "sub") { AckDeadlineSeconds = 20 }),
        ];
        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;
            return Task.FromResult(new Subscription());
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(configs, builder, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*sub*project*conflicting*");
        reads.Should().Be(0);
    }

    [Fact]
    public async Task ConfigureAsync_WhenEffectiveDeclarationsMatch_ShouldConfigureOnce()
    {
        // Arrange
        var reads = 0;
        var updates = 0;
        var message = new MessageConfig("topic", "sub") { DeadLetterTopic = "dlq" };
        PubSubConfig[] configs =
        [
            new("project", message),
            new("project", message with { AckDeadlineSeconds = 300, MaxDeliveryAttempts = 0 }),
        ];
        var builder = CreateClientBuilder((request, _) =>
        {
            reads++;
            return Task.FromResult(new Subscription { Name = request.Subscription });
        }, (request, _) =>
        {
            updates++;
            return Task.FromResult(request.Subscription);
        });

        // Act
        await PubSubConfigurator.ConfigureAsync(configs, builder, TestContext.Current.CancellationToken);

        // Assert
        reads.Should().Be(1);
        updates.Should().Be(1);
    }

    [Fact]
    public async Task ConfigureAsync_WhenPushDestinationIsMissing_ShouldFailBeforeRpcEvenWithReference()
    {
        // Arrange
        var app = DistributedApplication.CreateBuilder([]);
        var configs = new[] { new PubSubConfig("project", new MessageConfig("topic", "sub", "/callback")) };
        var pubSub = app.AddGcpPubSub("pubsub").WithPubSubConfigs(configs);

        app.AddProject<MVFC_Aspire_Helpers_Playground_Api>("consumer").WithReference(pubSub);

        var reads = 0;
        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;

            return Task.FromResult(new Subscription());
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(configs, builder, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*sub*PushEndpointReference*");

        reads.Should().Be(0);
        configs[0].MessageConfigs[0].PushEndpointReference.Should().BeNull();
    }

    [Fact]
    public async Task ConfigureAsync_WithMultipleDestinations_ShouldConfigureEachSubscriptionIndependently()
    {
        // Arrange
        var app = DistributedApplication.CreateBuilder([]);
        var first = app.AddProject<MVFC_Aspire_Helpers_Playground_Api>("first");
        var second = app.AddProject<MVFC_Aspire_Helpers_Playground_Api>("second");
        var firstEndpoint = first.Resource.Annotations.OfType<EndpointAnnotation>().Single(e => e.Name == "http");
        var secondEndpoint = second.Resource.Annotations.OfType<EndpointAnnotation>().Single(e => e.Name == "http");

        firstEndpoint.AllocatedEndpoint = new AllocatedEndpoint(firstEndpoint, "localhost", 5050);
        secondEndpoint.AllocatedEndpoint = new AllocatedEndpoint(secondEndpoint, "localhost", 6060);

        PubSubConfig[] configs =
        [
            new("first-project", new MessageConfig("topic", "sub", "/first") { PushEndpointReference = first.GetEndpoint("http") }),
            new("second-project", new MessageConfig("topic", "sub", "/second") { PushEndpointReference = second.GetEndpoint("http") }),
            new("pull-project", new MessageConfig("topic", "pull")),
        ];
        var subscriptions = new System.Collections.Concurrent.ConcurrentDictionary<string, Subscription>();
        var builder = CreateClientBuilder((request, _) => Task.FromResult(new Subscription { Name = request.Subscription }),
            (request, _) =>
            {
                subscriptions[request.Subscription.Name] = request.Subscription.Clone();

                return Task.FromResult(request.Subscription);
            });

        // Act
        await PubSubConfigurator.ConfigureAsync(configs, builder, TestContext.Current.CancellationToken);

        // Assert
        subscriptions.Should().HaveCount(3);
        subscriptions["projects/first-project/subscriptions/sub"].PushConfig.PushEndpoint.Should()
            .Be($"http://{PubSubDefaults.DockerInternalHost}:5050/first");
        subscriptions["projects/second-project/subscriptions/sub"].PushConfig.PushEndpoint.Should()
            .Be($"http://{PubSubDefaults.DockerInternalHost}:6060/second");
        subscriptions["projects/pull-project/subscriptions/pull"].PushConfig.Should().BeNull();
    }

    [Fact]
    public async Task ConfigureAsync_WhenDestinationIsNotAllocated_ShouldHonorDeadlineBeforeRpc()
    {
        // Arrange
        var app = DistributedApplication.CreateBuilder([]);
        var consumer = app.AddProject<MVFC_Aspire_Helpers_Playground_Api>("consumer");
        PubSubConfig[] configs =
            [new("project", new MessageConfig("topic", "sub", "/callback") { PushEndpointReference = consumer.GetEndpoint("http") })];
        var reads = 0;
        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;

            return Task.FromResult(new Subscription());
        });
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
        var task = PubSubConfigurator.ConfigureAsync(configs, builder, CancellationToken.None, timeProvider: time);
        var wasWaiting = !task.IsCompleted;

        callback!(state);

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        wasWaiting.Should().BeTrue();
        await act.Should().ThrowAsync<TimeoutException>();

        reads.Should().Be(0);
        time.Received(1).CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(),
            TimeSpan.FromSeconds(PubSubDefaults.CONFIGURATION_TIMEOUT_SECONDS), Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public async Task ConfigureAsync_WhenSuccessful_ShouldApplyExpectedConfiguration()
    {
        // Arrange
        UpdateSubscriptionRequest? update = null;

        var reads = 0;

        var builder = CreateClientBuilder((request, _) =>
        {
            reads++;

            return Task.FromResult(new Subscription { Name = request.Subscription });
        }, (request, _) =>
        {
            update = request;

            return Task.FromResult(request.Subscription);
        });

        // Act
        await PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        reads.Should().Be(1);
        update.Should().NotBeNull();
        update!.Subscription.Name.Should().Be("projects/project/subscriptions/sub");
        update.Subscription.PushConfig.PushEndpoint.Should().Be($"http://{PubSubDefaults.DockerInternalHost}:8080/callback");
        update.Subscription.AckDeadlineSeconds.Should().Be(300);
        update.Subscription.DeadLetterPolicy.DeadLetterTopic.Should().Be("projects/project/topics/dlq");
        update.UpdateMask.Paths.Should().BeEquivalentTo("ack_deadline_seconds", "push_config", "dead_letter_policy");
    }

    [Theory]
    [InlineData(StatusCode.NotFound)]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.Aborted)]
    [InlineData(StatusCode.Unknown)]
    public async Task ConfigureAsync_WhenReadFailsTemporarily_ShouldRecover(StatusCode status)
    {
        // Arrange
        var reads = 0;

        var updates = 0;

        var builder = CreateClientBuilder((request, _) => ++reads == 1
            ? Task.FromException<Subscription>(new RpcException(new Status(status, "temporary")))
            : Task.FromResult(new Subscription { Name = request.Subscription }),
            (request, _) =>
            {
                updates++;

                return Task.FromResult(request.Subscription);
            });

        // Act
        await PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        reads.Should().Be(2);
        updates.Should().Be(1);
    }

    [Theory]
    [InlineData(StatusCode.InvalidArgument)]
    [InlineData(StatusCode.PermissionDenied)]
    [InlineData(StatusCode.Unauthenticated)]
    public async Task ConfigureAsync_WhenReadFailsPermanently_ShouldNotRetry(StatusCode status)
    {
        // Arrange
        var reads = 0;

        var error = new RpcException(new Status(status, "permanent"));

        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;

            return Task.FromException<Subscription>(error);
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<RpcException>()).Which.Should().BeSameAs(error);
        reads.Should().Be(1);
    }

    [Fact]
    public async Task ConfigureAsync_WhenSubscriptionRemainsMissing_ShouldStopAtAttemptLimit()
    {
        // Arrange
        var reads = 0;

        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;

            return Task.FromException<Subscription>(new RpcException(new Status(StatusCode.NotFound, "missing")));
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<RpcException>()).Which.StatusCode.Should().Be(StatusCode.NotFound);
        reads.Should().Be(PubSubDefaults.CONFIGURATION_MAX_ATTEMPTS);
    }

    [Fact]
    public async Task ConfigureAsync_WhenUpdateIsUnavailable_ShouldUseSdkRetry()
    {
        // Arrange
        var updates = 0;

        var builder = CreateClientBuilder((request, _) => Task.FromResult(new Subscription { Name = request.Subscription }),
            (request, _) => ++updates == 1
                ? Task.FromException<Subscription>(new RpcException(new Status(StatusCode.Unavailable, "temporary")))
                : Task.FromResult(request.Subscription));

        // Act
        await PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        updates.Should().Be(2);
    }

    [Fact]
    public async Task ConfigureAsync_WhenUpdateIsMissing_ShouldNotRetry()
    {
        // Arrange
        var updates = 0;

        var builder = CreateClientBuilder((request, _) => Task.FromResult(new Subscription { Name = request.Subscription }),
            (_, _) =>
            {
                updates++;

                return Task.FromException<Subscription>(new RpcException(new Status(StatusCode.NotFound, "missing")));
            });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<RpcException>()).Which.StatusCode.Should().Be(StatusCode.NotFound);
        updates.Should().Be(1);
    }

    [Fact]
    public async Task ConfigureAsync_WhenAlreadyCancelled_ShouldNotCallEmulator()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var reads = 0;

        var builder = CreateClientBuilder((request, _) =>
        {
            reads++;

            return Task.FromResult(new Subscription { Name = request.Subscription });
        });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(_configs, builder, cancellation.Token);

        // Assert
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        reads.Should().Be(0);
    }

    [Fact]
    public async Task ConfigureAsync_WhenCancelledDuringBackoff_ShouldStopWaiting()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();

        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var jitter = Substitute.For<RetrySettings.IJitter>();

        jitter.GetDelay(Arg.Any<TimeSpan>()).Returns(_ =>
        {
            waiting.TrySetResult();

            return TimeSpan.FromSeconds(10);
        });

        var reads = 0;

        var builder = CreateClientBuilder((_, _) =>
        {
            reads++;

            return Task.FromException<Subscription>(new RpcException(new Status(StatusCode.NotFound, "starting")));
        }, jitter: jitter);

        // Act
        var task = PubSubConfigurator.ConfigureAsync(_configs, builder, cancellation.Token);

        await waiting.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        cancellation.Cancel();

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
        reads.Should().Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfigureAsync_WhenDeadlineExpires_ShouldCancelRpcAndReportTimeout(bool grpcCancellation)
    {
        // Arrange
        TimerCallback? callback = null;

        object? state = null;

        TimeSpan? dueTime = null;

        var time = Substitute.For<TimeProvider>();

        time.CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>())
            .Returns(call =>
            {
                callback = call.ArgAt<TimerCallback>(0);
                state = call.ArgAt<object?>(1);
                dueTime = call.ArgAt<TimeSpan>(2);

                return Substitute.For<ITimer>();
            });
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var logger = Substitute.For<ILogger>();

        logger.IsEnabled(LogLevel.Error).Returns(true);

        var builder = CreateClientBuilder(async (_, options) =>
        {
            started.TrySetResult();

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, options.CancellationToken);
            }
            catch (OperationCanceledException) when (grpcCancellation)
            {
                throw new RpcException(new Status(StatusCode.Cancelled, "cancelled"));
            }

            return new Subscription();
        });

        // Act
        var task = PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None, logger, time);

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        callback!(state);

        var act = () => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Assert
        callback.Should().NotBeNull();
        dueTime.Should().Be(TimeSpan.FromSeconds(PubSubDefaults.CONFIGURATION_TIMEOUT_SECONDS));

        var exception = (await act.Should().ThrowAsync<TimeoutException>()).Which;
        exception.Message.Should().Contain("60 seconds");
        exception.InnerException.Should().BeOfType(grpcCancellation ? typeof(RpcException) : typeof(TaskCanceledException));

        var log = logger.ReceivedCalls().Single(call => call.GetMethodInfo().Name == nameof(ILogger.Log));

        log.GetArguments()[0].Should().Be(LogLevel.Error);
        log.GetArguments()[2]!.ToString().Should().Be("Pub/Sub subscription configuration timed out after 60 seconds");
    }

    [Fact]
    public async Task ConfigureAsync_WhenSubscriptionFails_ShouldLogStructuredDiagnosticAndPreserveException()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();

        logger.IsEnabled(LogLevel.Error).Returns(true);

        var error = new RpcException(new Status(StatusCode.PermissionDenied, "access denied"));

        var builder = CreateClientBuilder((_, _) => Task.FromException<Subscription>(error));

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None, logger);

        // Assert
        (await act.Should().ThrowAsync<RpcException>()).Which.Should().BeSameAs(error);

        var log = logger.ReceivedCalls().Single(call => call.GetMethodInfo().Name == nameof(ILogger.Log));

        var arguments = log.GetArguments();
        arguments[0].Should().Be(LogLevel.Error);
        arguments[3].Should().BeSameAs(error);
        arguments[2]!.ToString().Should().Be("Failed to configure Pub/Sub subscription 'sub' in project 'project' (PermissionDenied)");

        var fields = (IReadOnlyList<KeyValuePair<string, object?>>)arguments[2]!;

        fields.Should().Contain(pair => pair.Key == "ProjectId" && Equals(pair.Value, "project"));
        fields.Should().Contain(pair => pair.Key == "Subscription" && Equals(pair.Value, "sub"));
        fields.Should().Contain(pair => pair.Key == "StatusCode" && Equals(pair.Value, StatusCode.PermissionDenied));
        fields.Should().Contain(pair => pair.Key == "{OriginalFormat}" && Equals(pair.Value,
            "Failed to configure Pub/Sub subscription '{Subscription}' in project '{ProjectId}' ({StatusCode})"));
        fields.Count.Should().Be(4);

        var beforeStart = () => fields[-1];
        var afterEnd = () => fields[fields.Count];

        beforeStart.Should().Throw<IndexOutOfRangeException>();
        afterEnd.Should().Throw<IndexOutOfRangeException>();

        var enumerated = ((System.Collections.IEnumerable)fields).Cast<KeyValuePair<string, object?>>().ToArray();

        enumerated.Should().BeEquivalentTo(fields);

        var untyped = ((System.Collections.IEnumerable)fields).GetEnumerator();
        var untypedFields = new List<object?>();

        while (untyped.MoveNext())
            untypedFields.Add(untyped.Current);

        untypedFields.Should().BeEquivalentTo(fields.Cast<object?>());

        var formatter = (Delegate)arguments[4]!;

        formatter.DynamicInvoke(arguments[2], arguments[3]).Should().Be(arguments[2]!.ToString());
    }

    [Fact]
    public async Task ConfigureAsync_WhenSuccessful_ShouldLogCompletion()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();

        logger.IsEnabled(LogLevel.Information).Returns(true);

        var builder = CreateClientBuilder((request, _) => Task.FromResult(new Subscription { Name = request.Subscription }),
            (request, _) => Task.FromResult(request.Subscription));

        // Act
        await PubSubConfigurator.ConfigureAsync(_configs, builder, CancellationToken.None, logger);

        // Assert
        var messages = logger.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
            .Select(call => call.GetArguments()[2]!.ToString());
        messages.Should().Contain("Configuring 1 Pub/Sub subscriptions");
        messages.Should().Contain(message => message!.StartsWith("Configured 1 Pub/Sub subscriptions in ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ConfigureAsync_WhenRepeatedAfterPartialFailure_ShouldConverge()
    {
        // Arrange
        PubSubConfig[] configs = [new("project", messageConfigs: [new("topic", "first"), new("topic", "second")])];
        var subscriptions = new System.Collections.Concurrent.ConcurrentDictionary<string, Subscription>();

        var failSecond = true;

        var builder = CreateClientBuilder((request, _) => Task.FromResult(new Subscription { Name = request.Subscription }),
            (request, _) =>
            {
                if (request.Subscription.Name.EndsWith("/second", StringComparison.Ordinal) && failSecond)
                    return Task.FromException<Subscription>(new RpcException(new Status(StatusCode.InvalidArgument, "failure")));
                subscriptions[request.Subscription.Name] = request.Subscription.Clone();
                return Task.FromResult(request.Subscription);
            });

        // Act
        var act = () => PubSubConfigurator.ConfigureAsync(configs, builder, CancellationToken.None);
        // Assert
        await act.Should().ThrowAsync<RpcException>();
        subscriptions.Should().ContainSingle();

        // Act
        failSecond = false;
        await PubSubConfigurator.ConfigureAsync(configs, builder, CancellationToken.None);

        // Assert
        subscriptions.Keys.Should().BeEquivalentTo("projects/project/subscriptions/first", "projects/project/subscriptions/second");
        subscriptions.Values.Should().OnlyContain(s => s.AckDeadlineSeconds == PubSubDefaults.ACK_DEADLINE_SECONDS_DEFAULT);
    }

    private static EndpointReference CreatePushEndpoint()
    {
        var app = DistributedApplication.CreateBuilder([]);
        var consumer = app.AddContainer("consumer", "unused").WithHttpEndpoint(targetPort: 80);
        var endpoint = consumer.Resource.Annotations.OfType<EndpointAnnotation>().Single();

        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 8080);

        return consumer.GetEndpoint("http");
    }

    private static SubscriberServiceApiClientBuilder CreateClientBuilder(
        Func<GetSubscriptionRequest, CallOptions, Task<Subscription>> get,
        Func<UpdateSubscriptionRequest, CallOptions, Task<Subscription>>? update = null,
        RetrySettings.IJitter? jitter = null)
    {
        var invoker = Substitute.For<CallInvoker>();

        invoker.AsyncUnaryCall(Arg.Any<Method<GetSubscriptionRequest, Subscription>>(), Arg.Any<string>(), Arg.Any<CallOptions>(), Arg.Any<GetSubscriptionRequest>())
            .Returns(call => UnaryCall(get(call.ArgAt<GetSubscriptionRequest>(3), call.ArgAt<CallOptions>(2))));
        invoker.AsyncUnaryCall(Arg.Any<Method<UpdateSubscriptionRequest, Subscription>>(), Arg.Any<string>(), Arg.Any<CallOptions>(), Arg.Any<UpdateSubscriptionRequest>())
            .Returns(call => UnaryCall(update is null
                ? Task.FromException<Subscription>(new InvalidOperationException("Unexpected subscription update."))
                : update(call.ArgAt<UpdateSubscriptionRequest>(3), call.ArgAt<CallOptions>(2))));

        jitter ??= Substitute.For<RetrySettings.IJitter>();
        var settings = PubSubConfigurator.CreateSubscriberSettings();
        settings.GetSubscriptionSettings = WithJitter(settings.GetSubscriptionSettings, jitter);
        settings.UpdateSubscriptionSettings = WithJitter(settings.UpdateSubscriptionSettings, jitter);

        return new SubscriberServiceApiClientBuilder { CallInvoker = invoker, Settings = settings };
    }

    private static CallSettings WithJitter(CallSettings settings, RetrySettings.IJitter jitter)
    {
        var retry = settings.Retry;

        return settings.WithRetry(RetrySettings.FromExponentialBackoff(
            retry.MaxAttempts, retry.InitialBackoff, retry.MaxBackoff, retry.BackoffMultiplier, retry.RetryFilter, jitter));
    }

    private static AsyncUnaryCall<Subscription> UnaryCall(Task<Subscription> response) =>
        new(response, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
}
