namespace MVFC.Aspire.Helpers.UnitTests.RabbitMQ;

public sealed class RabbitMQResourceTests
{
    [Fact]
    public void Constructor_ShouldSetName()
    {
        var resource = new RabbitMQResource("test-rabbitmq");

        resource.Name.Should().Be("test-rabbitmq");
    }

    [Fact]
    public void AmqpEndpoint_ShouldNotBeNull()
    {
        var resource = new RabbitMQResource("test-rabbitmq");

        resource.AmqpEndpoint.Should().NotBeNull();
    }

    [Fact]
    public void ConnectionStringExpression_ShouldNotBeNull()
    {
        var resource = new RabbitMQResource("test-rabbitmq");

        resource.ConnectionStringExpression.Should().NotBeNull();
    }
}
