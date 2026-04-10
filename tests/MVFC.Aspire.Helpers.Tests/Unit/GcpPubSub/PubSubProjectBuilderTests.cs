namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

public sealed class PubSubProjectBuilderTests
{
    [Fact]
    public void Build_ShouldReturnOnlyProjectId_WhenNoMessageConfigs()
    {
        // Arrange
        var config = new PubSubConfig("my-project");

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project");
    }

    [Fact]
    public void Build_ShouldThrow_WhenPubSubProjectIsNull()
    {
        // Arrange
        PubSubConfig config = null!;

        // Act
        var act = () => PubSubProjectBuilder.Build(config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Build_ShouldReturnProjectIdAndTopic_WhenNoSubscription()
    {
        // Arrange
        var config = new PubSubConfig("my-project", new MessageConfig("my-topic"));

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,my-topic");
    }

    [Fact]
    public void Build_ShouldReturnTopicAndSubscription_WhenBothProvided()
    {
        // Arrange
        var config = new PubSubConfig("my-project", new MessageConfig("my-topic", "my-sub"));

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,my-topic:my-sub");
    }

    [Fact]
    public void Build_ShouldJoinMultipleTopicsWithComma()
    {
        // Arrange
        var config = new PubSubConfig("my-project", secondsDelay: 5,
            messageConfigs:
            [
                new MessageConfig("topic-a", "sub-a"),
                new MessageConfig("topic-b", "sub-b")
            ]);

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,topic-a:sub-a,topic-b:sub-b");
    }

    [Fact]
    public void Build_ShouldHandleMixedTopicsWithAndWithoutSubscription()
    {
        // Arrange
        var config = new PubSubConfig("my-project", secondsDelay: 5,
            messageConfigs:
            [
                new MessageConfig("topic-only"),
                new MessageConfig("topic-with-sub", "my-sub")
            ]);

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,topic-only,topic-with-sub:my-sub");
    }

    [Fact]
    public void Build_ShouldAppendDeadLetterTopicAndSubscription_WhenDeadLetterTopicDefined()
    {
        // Arrange
        var config = new PubSubConfig("my-project",
            new MessageConfig("my-topic", "my-sub")
            {
                DeadLetterTopic = "my-dlq"
            });

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        // formato: projeto,topico:sub,dlq:dlq-subscription
        result.Should().Be("my-project,my-topic:my-sub,my-dlq:my-dlq-subscription");
    }

    [Fact]
    public void Build_ShouldNotAppendDeadLetter_WhenDeadLetterTopicIsNull()
    {
        // Arrange
        var config = new PubSubConfig("my-project",
            new MessageConfig("my-topic", "my-sub") { DeadLetterTopic = null });

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,my-topic:my-sub");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Build_ShouldNotAppendDeadLetter_WhenDeadLetterTopicIsNullOrWhitespace(string? dlq)
    {
        // Arrange
        var config = new PubSubConfig("my-project",
            new MessageConfig("my-topic", "my-sub") { DeadLetterTopic = dlq });

        // Act
        var result = PubSubProjectBuilder.Build(config);

        // Assert
        result.Should().Be("my-project,my-topic:my-sub");
    }
}
