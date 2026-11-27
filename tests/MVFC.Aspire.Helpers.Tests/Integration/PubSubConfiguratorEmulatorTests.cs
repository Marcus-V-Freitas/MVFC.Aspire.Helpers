namespace MVFC.Aspire.Helpers.Tests.Integration;

[Collection("PubSub emulator environment")]
public sealed class PubSubConfiguratorEmulatorTests
{
    [Fact]
    public async Task ModifyPushEndpoint_WithEmulatorEnvironment_ShouldPersistSubscriptionUpdate()
    {
        // Arrange
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromMinutes(2));
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            AssemblyName = "MVFC.Aspire.Helpers.Playground.AppHost",
            DisableDashboard = true,
            AllowUnsecuredTransport = true,
        });
        var emulator = builder.AddGcpPubSub("pubsub-configuration", port: 0);
        await using var app = builder.Build();
        await app.StartAsync(deadline.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync(emulator.Resource.Name, deadline.Token);

        var endpoint = await app.GetConnectionStringAsync(emulator.Resource.Name, deadline.Token);
        var publisher = await new PublisherServiceApiClientBuilder
        {
            Endpoint = endpoint,
            ChannelCredentials = ChannelCredentials.Insecure,
        }.BuildAsync(deadline.Token);
        var subscriber = await new SubscriberServiceApiClientBuilder
        {
            Endpoint = endpoint,
            ChannelCredentials = ChannelCredentials.Insecure,
        }.BuildAsync(deadline.Token);
        var topicName = TopicName.FromProjectTopic("coverage-project", "test-topic");
        var subscriptionName = SubscriptionName.FromProjectSubscription("coverage-project", "test-subscription");
        await publisher.CreateTopicAsync(topicName, deadline.Token);
        await subscriber.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 10, deadline.Token);

        var previous = Environment.GetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR);
        var message = new MessageConfig("test-topic", "test-subscription", "/callback") { AckDeadlineSeconds = 20 };

        try
        {
            Environment.SetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR, endpoint);

            // Act
            await PubSubConfigurator.ModifyPushEndpoint("coverage-project", message, "http://localhost:1234", deadline.Token);
            var subscription = await subscriber.GetSubscriptionAsync(subscriptionName, deadline.Token);

            // Assert
            subscription.Name.Should().Be(subscriptionName.ToString());
            subscription.AckDeadlineSeconds.Should().Be(20);
            subscription.PushConfig.PushEndpoint.Should().Be("http://localhost:1234/callback");
        }
        finally
        {
            Environment.SetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR, previous);
        }
    }
}
