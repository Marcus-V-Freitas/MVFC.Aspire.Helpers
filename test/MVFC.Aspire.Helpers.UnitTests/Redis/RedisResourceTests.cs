namespace MVFC.Aspire.Helpers.UnitTests.Redis;

public sealed class RedisResourceTests
{
    [Fact]
    public void Constructor_ShouldSetName()
    {
        var resource = new RedisResource("test-redis");

        resource.Name.Should().Be("test-redis");
    }

    [Fact]
    public void RedisEndpoint_ShouldNotBeNull()
    {
        var resource = new RedisResource("test-redis");

        resource.RedisEndpoint.Should().NotBeNull();
    }

    [Fact]
    public void ConnectionStringExpression_ShouldNotBeNull()
    {
        var resource = new RedisResource("test-redis");

        resource.ConnectionStringExpression.Should().NotBeNull();
    }
}
