namespace MVFC.Aspire.Helpers.Tests.Unit.WireMock;

public sealed class WireMockExtensionsTests : IDisposable
{
    private WireMockServer? _server;

    public void Dispose() => _server?.Stop();

    [Fact]
    public void AddWireMock_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddWireMock("wiremock");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddWireMock_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddWireMock(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddWireMock_ShouldCreateResourceWithServer()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var wmBuilder = builder.AddWireMock("wiremock");

        // Assert
        wmBuilder.Should().NotBeNull();
        wmBuilder.Resource.Name.Should().Be("wiremock");
        wmBuilder.Resource.Server.Should().NotBeNull();
        wmBuilder.Resource.Server.IsStarted.Should().BeTrue();
        wmBuilder.Resource.Port.Should().BeGreaterThan(0);

        _server = wmBuilder.Resource.Server;
    }

    [Fact]
    public void AddWireMock_WithConfigure_ShouldInvokeConfigureAction()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var configureInvoked = false;

        // Act
        var wmBuilder = builder.AddWireMock("wiremock", configure: server =>
        {
            configureInvoked = true;
        });

        // Assert
        configureInvoked.Should().BeTrue();
        _server = wmBuilder.Resource.Server;
    }

    [Fact]
    public void Endpoint_ShouldCreateEndpointBuilder()
    {
        // Arrange
        var server = WireMockServer.Start();

        // Act
        var endpointBuilder = server.Endpoint("/api/test");

        // Assert
        endpointBuilder.Should().NotBeNull();
        server.Stop();
    }

    [Fact]
    public void WireMockResource_ConnectionStringExpression_ShouldContainPort()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var wmBuilder = builder.AddWireMock("wiremock");
        var resource = wmBuilder.Resource;
        _server = resource.Server;

        // Act
        var connStr = resource.ConnectionStringExpression.ValueExpression;

        // Assert
        connStr.Should().Contain(resource.Port.ToString(CultureInfo.InvariantCulture));
    }
}
