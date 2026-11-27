namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class ContainerImageDefaultsTests
{
    [Theory]
    [InlineData("storage", "797ce226d62f947c009dc40246b30cfb456b8473d8241407f9d6f2c04e4d69ef")]
    [InlineData("bigquery", "f4e428d265a93dc5ce36c294e1c584c7c9b384117d47ab8ddbb63d8d50b7f393")]
    [InlineData("firestore", "74dc4be151bd8849117f00dfc0ff329be1919527261916893ef199a04a044137")]
    [InlineData("pubsub", "142fff1a0fa63e23559e86de8f39774e342c1654b533fd15532a495adabe9626")]
    [InlineData("pubsub-ui", "df024580ba3b3f091584e785c7ad71816a77d5730b0e514e4ba7598ddb10aa2c")]
    [InlineData("spanner", "18a56fd557011e50e1733a9232e8d17ec9bdd7e51f6cf7660f14c234479f4f36")]
    [InlineData("mailpit", "c96991d9bef73594c246d89ca81411d4e916f03e76a7d2d72fa2ab5dd3c9ce24")]
    [InlineData("mongo", "e0ce8c35124d4a9f9785532d1f268f39e9728ffa1cb38f46fa482436424c4bd3")]
    [InlineData("redis", "298e5b3bc566bade82f46ad5511777a4a07a294097ce16ada2f6a42be5239df5")]
    public void AddResource_ShouldPinTheValidatedImage(string name, string expectedDigest)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var resource = CreateResource(builder, name);
        var image = resource.Resource.Annotations.OfType<ContainerImageAnnotation>().Single();

        // Assert
        image.SHA256.Should().Be(expectedDigest);
        image.Tag.Should().BeNull();
    }

    [Theory]
    [InlineData("storage")]
    [InlineData("bigquery")]
    [InlineData("firestore")]
    [InlineData("pubsub")]
    [InlineData("pubsub-ui")]
    [InlineData("spanner")]
    [InlineData("mailpit")]
    [InlineData("mongo")]
    [InlineData("redis")]
    public void WithImage_ShouldAllowReplacingTheDefaultDigest(string name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var resource = CreateResource(builder, name);

        // Act
        resource.WithImage("custom/image", "test-version");
        var image = resource.Resource.Annotations.OfType<ContainerImageAnnotation>().Single();

        // Assert
        image.Image.Should().Be("custom/image");
        image.Tag.Should().Be("test-version");
        image.SHA256.Should().BeNull();
    }

    [Theory]
    [InlineData("ghcr.io/joeferner/redis-commander", "latest", true)]
    [InlineData("custom/commander", "latest", false)]
    [InlineData("ghcr.io/joeferner/redis-commander", "custom-version", false)]
    public void WithCommander_ShouldPinOnlyDefaultImageAndTag(string imageName, string tag, bool pinned)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var redis = builder.AddRedis("redis");

        // Act
        redis.WithCommander(image: imageName, tag: tag);
        var commander = builder.Resources.Single(resource => resource.Name == "redis-commander");
        var image = commander.Annotations.OfType<ContainerImageAnnotation>().Single();

        // Assert
        image.Image.Should().Be(imageName);
        image.SHA256.Should().Be(pinned ? "1ad484c1fc0a3b4072dfa2bb10d44f0eba8ecb342c8c0f9e17cb21a6557e4b7e" : null);
        image.Tag.Should().Be(pinned ? null : tag);
    }

    private static IResourceBuilder<ContainerResource> CreateResource(IDistributedApplicationBuilder builder, string name) =>
        name switch
        {
            "storage" => builder.AddCloudStorage(name),
            "bigquery" => builder.AddGcpBigQuery(name),
            "firestore" => builder.AddGcpFirestore(name),
            "pubsub" => builder.AddGcpPubSub(name),
            "pubsub-ui" => builder.AddGcpPubSubUI(name),
            "spanner" => builder.AddGcpSpanner(name),
            "mailpit" => builder.AddMailpit(name),
            "mongo" => builder.AddMongoReplicaSet(name),
            "redis" => builder.AddRedis(name),
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
}
