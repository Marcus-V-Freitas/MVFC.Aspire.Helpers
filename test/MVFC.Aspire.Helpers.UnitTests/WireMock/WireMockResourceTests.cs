namespace MVFC.Aspire.Helpers.UnitTests.WireMock;

public sealed class WireMockResourceTests {
    [Fact]
    public void Constructor_ShouldSetNameAndPort() {
        using var resource = new WireMockResource("wiremock-test", 0);

        resource.Name.Should().Be("wiremock-test");
    }

    [Fact]
    public void Server_ShouldBeRunning() {
        using var resource = new WireMockResource("wiremock-test", 0);

        resource.Server.Should().NotBeNull();
        resource.Server.IsStarted.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldStopServer() {
        var resource = new WireMockResource("wiremock-test", 0);
        resource.Server.IsStarted.Should().BeTrue();

        resource.Dispose();

        resource.Server.IsStarted.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithConfigure_ShouldInvokeAction() {
        var configureWasCalled = false;

        using var resource = new WireMockResource("wiremock-test", 0, server => {
            configureWasCalled = true;
        });

        configureWasCalled.Should().BeTrue();
    }
}
