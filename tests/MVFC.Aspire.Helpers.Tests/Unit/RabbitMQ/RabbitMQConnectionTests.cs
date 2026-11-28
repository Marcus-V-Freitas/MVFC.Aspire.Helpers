namespace MVFC.Aspire.Helpers.Tests.Unit.RabbitMQ;

public sealed class RabbitMQConnectionTests
{
    [Theory]
    [InlineData("user", "secret")]
    [InlineData("user @:/%,雪", "p @:/%,\"雪")]
    public async Task WithReference_ShouldResolveFinalCredentials(string username, string password)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var rabbit = builder.AddRabbitMQ("rabbit").WithCredentials("old-user", "old-secret");
        var consumer = builder.AddTestProject("consumer").WithReference(rabbit);
        var endpoint = rabbit.Resource.Annotations.OfType<EndpointAnnotation>().Single(e => e.Name == "amqp");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 5672);
        rabbit.WithCredentials(username, password);

        // Act
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, TestContext.Current.CancellationToken);
        var value = await ((IValueProvider)environment["ConnectionStrings__rabbit"]).GetValueAsync(TestContext.Current.CancellationToken);
        var factory = new RabbitConnectionFactory { Uri = new Uri(value!) };

        // Assert
        factory.UserName.Should().Be(username);
        factory.Password.Should().Be(password);
        factory.VirtualHost.Should().Be("/");
        rabbit.Resource.PasswordParameter.Secret.Should().BeTrue();
        rabbit.Resource.ConnectionStringExpression.ValueExpression.Should().NotContain(password);
    }
}
