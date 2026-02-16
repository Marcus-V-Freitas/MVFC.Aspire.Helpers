namespace MVFC.Aspire.Helpers.UnitTests.Redis;

public sealed class RedisConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new RedisConfig();

        config.Port.Should().BeNull();
        config.Password.Should().BeNull();
        config.WithCommander.Should().BeFalse();
        config.CommanderPort.Should().BeNull();
        config.VolumeName.Should().BeNull();
        config.ImageName.Should().Be("redis");
        config.ImageTag.Should().Be("latest");
        config.CommanderImageName.Should().Be("ghcr.io/joeferner/redis-commander");
        config.CommanderImageTag.Should().Be("latest");
    }

    [Fact]
    public void CustomValues_ShouldOverrideDefaults()
    {
        var config = new RedisConfig(
            Port: 6379,
            Password: "my-password",
            WithCommander: true,
            CommanderPort: 8081,
            VolumeName: "redis-data",
            ImageName: "redis",
            ImageTag: "7-alpine",
            CommanderImageName: "custom/redis-commander",
            CommanderImageTag: "v1");

        config.Port.Should().Be(6379);
        config.Password.Should().Be("my-password");
        config.WithCommander.Should().BeTrue();
        config.CommanderPort.Should().Be(8081);
        config.VolumeName.Should().Be("redis-data");
        config.ImageName.Should().Be("redis");
        config.ImageTag.Should().Be("7-alpine");
        config.CommanderImageName.Should().Be("custom/redis-commander");
        config.CommanderImageTag.Should().Be("v1");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var config1 = new RedisConfig(Port: 6379);
        var config2 = new RedisConfig(Port: 6379);

        config1.Should().Be(config2);
    }
}
