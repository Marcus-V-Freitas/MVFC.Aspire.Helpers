using MVFC.Aspire.Helpers.GcpPubSub.Models;

namespace MVFC.Aspire.Helpers.UnitTests.GcpPubSub;

public class EmulatorConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new EmulatorConfig("pubsub");

        config.EmulatorName.Should().Be("pubsub");
        config.UiName.Should().Be("pubsub-ui");
        config.EmulatorImage.Should().Be("thekevjames/gcloud-pubsub-emulator");
        config.EmulatorTag.Should().Be("latest");
        config.UiImage.Should().Be("echocode/gcp-pubsub-emulator-ui");
        config.UiTag.Should().Be("latest");
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults()
    {
        var config = new EmulatorConfig(
            EmulatorName: "custom-pubsub",
            UiName: "custom-ui",
            EmulatorImage: "my-emulator",
            EmulatorTag: "v2",
            UiImage: "my-ui",
            UiTag: "v3");

        config.EmulatorName.Should().Be("custom-pubsub");
        config.UiName.Should().Be("custom-ui");
        config.EmulatorImage.Should().Be("my-emulator");
        config.EmulatorTag.Should().Be("v2");
        config.UiImage.Should().Be("my-ui");
        config.UiTag.Should().Be("v3");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new EmulatorConfig("pubsub");
        var config2 = new EmulatorConfig("pubsub");

        config1.Should().Be(config2);
    }
}
