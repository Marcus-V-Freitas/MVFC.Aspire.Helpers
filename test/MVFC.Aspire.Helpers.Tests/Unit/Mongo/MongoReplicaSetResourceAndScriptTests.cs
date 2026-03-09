namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoReplicaSetResourceAndScriptTests
{
    [Fact]
    public void MongoEndpoint_ShouldReturnSameInstance()
    {
        // Arrange
        var resource = new MongoReplicaSetResource("mongo");

        // Act
        var endpoint1 = resource.MongoEndpoint;
        var endpoint2 = resource.MongoEndpoint;

        // Assert
        endpoint1.Should().BeSameAs(endpoint2);
        endpoint1.EndpointName.Should().Be("mongodb");
    }

    [Fact]
    public void ConnectionStringExpression_ShouldContainDirectConnectionTrue()
    {
        // Arrange
        var resource = new MongoReplicaSetResource("mongo");

        // Act
        var expression = resource.ConnectionStringExpression.ValueExpression;

        // Assert
        expression.Should().Contain("mongodb://");
        expression.Should().Contain("directConnection=true");
    }

    [Fact]
    public void GetInitScript_ShouldLoadEmbeddedResource()
    {
        // Act
        var script = ReplicaSetScriptProvider.GetInitScript();

        // Assert
        script.Should().NotBeNullOrWhiteSpace();
        script.Should().Contain("rs.initiate");
        script.Should().Contain("Replica set initiated");
    }
}
