using MVFC.Aspire.Helpers.CloudStorage.Resources;

namespace MVFC.Aspire.Helpers.UnitTests.CloudStorage;

public class CloudStorageResourceTests {
    [Fact]
    public void Constructor_ShouldSetName() {
        var resource = new CloudStorageResource("gcs-test");

        resource.Name.Should().Be("gcs-test");
    }

    [Fact]
    public void HttpEndpoint_ShouldReturnSameReference() {
        var resource = new CloudStorageResource("gcs-test");

        var endpoint1 = resource.HttpEndpoint;
        var endpoint2 = resource.HttpEndpoint;

        endpoint1.Should().BeSameAs(endpoint2);
    }

    [Fact]
    public void ConnectionStringExpression_ShouldNotBeNull() {
        var resource = new CloudStorageResource("gcs-test");

        resource.ConnectionStringExpression.Should().NotBeNull();
    }
}
