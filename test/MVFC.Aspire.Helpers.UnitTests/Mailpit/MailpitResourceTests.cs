namespace MVFC.Aspire.Helpers.UnitTests.Mailpit;

public sealed class MailpitResourceTests {
    [Fact]
    public void Constructor_ShouldSetName() {
        var resource = new MailpitResource("mailpit-test");

        resource.Name.Should().Be("mailpit-test");
    }

    [Fact]
    public void SmtpEndpoint_ShouldReturnSameReference() {
        var resource = new MailpitResource("mailpit-test");

        var endpoint1 = resource.SmtpEndpoint;
        var endpoint2 = resource.SmtpEndpoint;

        endpoint1.Should().BeSameAs(endpoint2);
    }

    [Fact]
    public void ConnectionStringExpression_ShouldNotBeNull() {
        var resource = new MailpitResource("mailpit-test");

        resource.ConnectionStringExpression.Should().NotBeNull();
    }
}
