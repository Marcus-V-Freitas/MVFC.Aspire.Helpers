namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubConfiguratorTests
{
    [Fact]
    public void BuildDeadLetterPolicy_WhenDeadLetterTopicIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub") { DeadLetterTopic = null };

        // Act
        var result = PubSubConfigurator.BuildDeadLetterPolicy("project", messageConfig);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildDeadLetterPolicy_WhenDeadLetterTopicIsSet_ShouldReturnPolicy()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub")
        {
            DeadLetterTopic = "dlq",
            MaxDeliveryAttempts = 10
        };

        // Act
        var result = PubSubConfigurator.BuildDeadLetterPolicy("project", messageConfig);

        // Assert
        result.Should().NotBeNull();
        result!.DeadLetterTopic.Should().Be(TopicName.FormatProjectTopic("project", "dlq"));
        result.MaxDeliveryAttempts.Should().Be(10);
    }

    [Fact]
    public void BuildFieldMaskUpdate_ShouldIncludeRequiredPaths()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub")
        {
            PushEndpoint = "endpoint",
            DeadLetterTopic = "dlq"
        };

        // Act
        var result = PubSubConfigurator.BuildFieldMaskUpdate(messageConfig);

        // Assert
        result.Paths.Should().Contain("ack_deadline_seconds");
        result.Paths.Should().Contain("push_config");
        result.Paths.Should().Contain("dead_letter_policy");
    }

    [Fact]
    public void BuildFieldMaskUpdate_WhenOptionalFieldsMissing_ShouldOnlyIncludeBase()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub")
        {
            PushEndpoint = null,
            DeadLetterTopic = null
        };

        // Act
        var result = PubSubConfigurator.BuildFieldMaskUpdate(messageConfig);

        // Assert
        result.Paths.Should().ContainSingle().Which.Should().Be("ack_deadline_seconds");
    }

    [Theory]
    [InlineData("http://localhost:1234", "callback", "http://localhost:1234/callback")]
    [InlineData("http://localhost:1234/", "/callback", "http://localhost:1234/callback")]
    public void BuildPushEndpoint_ShouldFormatCorrectUrl(string baseUri, string endpoint, string expected)
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub") { PushEndpoint = endpoint };

        // Act
        var result = PubSubConfigurator.BuildPushEndpoint(messageConfig, baseUri);

        // Assert
        result.Should().NotBeNull();
        result!.PushEndpoint.Should().Be(expected);
    }

    [Fact]
    public void BuildPushEndpoint_WhenPushEndpointIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var messageConfig = new MessageConfig("topic", "sub") { PushEndpoint = null };

        // Act
        var result = PubSubConfigurator.BuildPushEndpoint(messageConfig, "http://localhost");

        // Assert
        result.Should().BeNull();
    }
}
