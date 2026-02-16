namespace MVFC.Aspire.Helpers.UnitTests.CloudStorage;

public sealed class CloudStorageConfigTests {
    [Fact]
    public void DefaultValues_ShouldBeCorrect() {
        var config = new CloudStorageConfig();

        config.LocalBucketFolder.Should().BeNull();
        config.EmulatorImage.Should().Be("fsouza/fake-gcs-server");
        config.EmulatorTag.Should().Be("latest");
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults() {
        var config = new CloudStorageConfig(
            LocalBucketFolder: "/data/buckets",
            EmulatorImage: "custom-image",
            EmulatorTag: "v1.0");

        config.LocalBucketFolder.Should().Be("/data/buckets");
        config.EmulatorImage.Should().Be("custom-image");
        config.EmulatorTag.Should().Be("v1.0");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly() {
        var config1 = new CloudStorageConfig(EmulatorImage: "img", EmulatorTag: "tag");
        var config2 = new CloudStorageConfig(EmulatorImage: "img", EmulatorTag: "tag");

        config1.Should().Be(config2);
    }
}
