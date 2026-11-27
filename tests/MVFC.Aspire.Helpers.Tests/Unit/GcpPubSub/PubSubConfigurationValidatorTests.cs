namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubConfigurationValidatorTests
{
    [Fact]
    public void GetSubscriptions_WhenCollectionIsNull_ShouldRejectIt()
    {
        // Arrange
        IReadOnlyList<PubSubConfig> configs = null!;

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void GetSubscriptions_WhenNestedConfigurationIsNull_ShouldRejectIt(int scenario)
    {
        // Arrange
        PubSubConfig[] configs = scenario switch
        {
            0 => [null!],
            1 => [new("project") { MessageConfigs = null! }],
            _ => [new("project", messageConfigs: [null!])],
        };

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetSubscriptions_WhenIdentifierIsMissing_ShouldRejectProjectAndTopic(string? identifier)
    {
        // Arrange
        PubSubConfig[] invalidProject = [new(identifier!, new MessageConfig("topic", "sub"))];
        PubSubConfig[] invalidTopic = [new("project", new MessageConfig(identifier!, "sub"))];

        // Act
        var validateProject = () => PubSubConfigurationValidator.GetSubscriptions(invalidProject);
        var validateTopic = () => PubSubConfigurationValidator.GetSubscriptions(invalidTopic);

        // Assert
        validateProject.Should().Throw<ArgumentException>();
        validateTopic.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetSubscriptions_WhenTopicHasNoSubscription_ShouldSkipPolicyValidation(string? subscription)
    {
        // Arrange
        PubSubConfig[] configs = [new("project", new MessageConfig("topic", subscription) { AckDeadlineSeconds = -1 })];

        // Act
        var result = PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSubscriptions_WhenPushHasNoSubscription_ShouldRejectIt()
    {
        // Arrange
        PubSubConfig[] configs = [new("project", new MessageConfig("topic", PushEndpoint: "/callback"))];

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*SubscriptionName*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(300)]
    [InlineData(600)]
    public void GetSubscriptions_WhenAckDeadlineIsSupported_ShouldAcceptIt(int? seconds)
    {
        // Arrange
        var message = new MessageConfig("topic", "sub") { AckDeadlineSeconds = seconds };
        PubSubConfig[] configs = [new("project", message)];

        // Act
        var result = PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        result.Should().ContainSingle().Which.MessageConfig.Should().BeSameAs(message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    [InlineData(601)]
    public void GetSubscriptions_WhenAckDeadlineIsOutsideServiceRange_ShouldRejectIt(int seconds)
    {
        // Arrange
        PubSubConfig[] configs = [new("project", new MessageConfig("topic", "sub") { AckDeadlineSeconds = seconds })];

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*AckDeadlineSeconds*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    public void GetSubscriptions_WhenDeliveryAttemptsAreSupported_ShouldAcceptThem(int? attempts)
    {
        // Arrange
        var message = new MessageConfig("topic", "sub") { DeadLetterTopic = "dlq", MaxDeliveryAttempts = attempts };
        PubSubConfig[] configs = [new("project", message)];

        // Act
        var result = PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        result.Should().ContainSingle().Which.MessageConfig.Should().BeSameAs(message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(101)]
    public void GetSubscriptions_WhenDeliveryAttemptsAreOutsideServiceRange_ShouldRejectOnlyWithDlq(int attempts)
    {
        // Arrange
        var message = new MessageConfig("topic", "sub") { MaxDeliveryAttempts = attempts };
        PubSubConfig[] withDlq = [new("project", message with { DeadLetterTopic = "dlq" })];
        PubSubConfig[] withoutDlq = [new("project", message)];

        // Act
        var validateDlq = () => PubSubConfigurationValidator.GetSubscriptions(withDlq);
        var result = PubSubConfigurationValidator.GetSubscriptions(withoutDlq);

        // Assert
        validateDlq.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*MaxDeliveryAttempts*");
        result.Should().ContainSingle().Which.MessageConfig.Should().BeSameAs(message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GetSubscriptions_WhenDeclarationsHaveSameEffectivePolicies_ShouldCoalesceThem(int scenario)
    {
        // Arrange
        var first = new MessageConfig("topic", "sub");
        var second = first with { AckDeadlineSeconds = 300, DeadLetterTopic = " ", PushEndpoint = " ", MaxDeliveryAttempts = -1 };

        if (scenario == 1)
        {
            first = first with { AckDeadlineSeconds = 0 };
            second = first with { AckDeadlineSeconds = 10 };
        }
        else if (scenario >= 2)
        {
            first = first with { DeadLetterTopic = "dlq" };
            second = first with { MaxDeliveryAttempts = scenario == 2 ? 0 : 5 };
        }

        PubSubConfig[] configs = [new("project", first), new("project", second)];

        // Act
        var result = PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        result.Should().ContainSingle().Which.MessageConfig.Should().BeSameAs(first);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void GetSubscriptions_WhenSubscriptionPoliciesConflict_ShouldRejectThem(int scenario)
    {
        // Arrange
        var first = new MessageConfig("topic", "sub") { DeadLetterTopic = "dlq" };
        var second = scenario switch
        {
            0 => first with { TopicName = "other" },
            1 => first with { AckDeadlineSeconds = 10 },
            2 => first with { DeadLetterTopic = "other" },
            3 => first with { MaxDeliveryAttempts = 6 },
            _ => first with { PushEndpoint = "/callback", PushEndpointReference = CreateEndpoint() },
        };
        PubSubConfig[] configs = [new("project", first), new("project", second)];

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*sub*project*conflicting*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GetSubscriptions_WhenPushDestinationIsCompared_ShouldUseResourceAndEndpointIdentity(int scenario)
    {
        // Arrange
        var app = DistributedApplication.CreateBuilder([]);
        var consumer = app.AddContainer("consumer", "image").WithHttpEndpoint(targetPort: 80).WithHttpsEndpoint(targetPort: 443);
        var other = app.AddContainer("other", "image").WithHttpEndpoint(targetPort: 80);
        var first = new MessageConfig("topic", "sub", "/callback") { PushEndpointReference = consumer.GetEndpoint("http") };
        var second = scenario switch
        {
            0 => first with { PushEndpoint = "callback", PushEndpointReference = consumer.GetEndpoint("http") },
            1 => first with { PushEndpointReference = other.GetEndpoint("http") },
            2 => first with { PushEndpointReference = consumer.GetEndpoint("https") },
            _ => first with { PushEndpoint = "/other" },
        };
        PubSubConfig[] configs = [new("project", first), new("project", second)];

        // Act
        var act = () => PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        if (scenario == 0)
            act().Should().ContainSingle();
        else
            act.Should().Throw<InvalidOperationException>().WithMessage("*conflicting*");

        first.PushEndpointReference.IsAllocated.Should().BeFalse();
    }

    [Fact]
    public void GetSubscriptions_WhenProjectsOrSubscriptionsDiffer_ShouldKeepEachDeclaration()
    {
        // Arrange
        PubSubConfig[] configs =
        [
            new("project", new MessageConfig("topic", "sub")),
            new("other", new MessageConfig("topic", "sub")),
            new("project", new MessageConfig("topic", "other")),
        ];

        // Act
        var result = PubSubConfigurationValidator.GetSubscriptions(configs);

        // Assert
        result.Should().HaveCount(3);
    }

    private static EndpointReference CreateEndpoint() =>
        DistributedApplication.CreateBuilder([]).AddContainer("consumer", "image").WithHttpEndpoint(targetPort: 80).GetEndpoint("http");
}
