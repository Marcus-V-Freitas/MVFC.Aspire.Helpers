namespace MVFC.Aspire.Helpers.Tests.Unit.Redis;

public sealed class RedisExtensionsTests
{
    [Fact]
    public void AddRedis_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IDistributedApplicationBuilder? builder = null;

        // Act
        var act = () => builder!.AddRedis("redis");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddRedis_ShouldThrow_WhenNameIsNullOrWhitespace(string? name)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var act = () => builder.AddRedis(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRedis_ShouldConfigureResource_WithDefaults()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);

        // Act
        var redisBuilder = builder.AddRedis("redis-cache");

        // Assert
        redisBuilder.Should().NotBeNull();
        redisBuilder.Resource.Name.Should().Be("redis-cache");
    }

    [Fact]
    public void WithDockerImage_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RedisResource>? builder = null;

        // Act
        var act = () => builder!.WithDockerImage("redis", "latest");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "latest")]
    [InlineData("", "latest")]
    [InlineData(" ", "latest")]
    [InlineData("redis", null)]
    [InlineData("redis", "")]
    [InlineData("redis", " ")]
    public void WithDockerImage_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithDockerImage(image!, tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDockerImage_ShouldNotThrow_WhenParametersAreValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithDockerImage("redis", "latest");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithPassword_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RedisResource>? builder = null;

        // Act
        var act = () => builder!.WithPassword("secret");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithPassword_ShouldThrow_WhenPasswordInvalid(string? password)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithPassword(password!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithPassword_ShouldNotThrow_WhenPasswordIsValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithPassword("StrongPassword!123");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithCommander_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RedisResource>? builder = null;

        // Act
        var act = () => builder!.WithCommander();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "latest")]
    [InlineData("", "latest")]
    [InlineData(" ", "latest")]
    [InlineData("ghcr.io/joeferner/redis-commander", null)]
    [InlineData("ghcr.io/joeferner/redis-commander", "")]
    [InlineData("ghcr.io/joeferner/redis-commander", " ")]
    public void WithCommander_ShouldThrow_WhenImageOrTagInvalid(string? image, string? tag)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithCommander(image: image!, tag: tag!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithCommander_ShouldNotThrow_WithDefaultParameters()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithCommander();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithDataVolume_ShouldThrow_WhenBuilderIsNull()
    {
        // Arrange
        IResourceBuilder<RedisResource>? builder = null;

        // Act
        var act = () => builder!.WithDataVolume("redis-data");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WithDataVolume_ShouldThrow_WhenVolumeNameInvalid(string? volumeName)
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithDataVolume(volumeName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithDataVolume_ShouldNotThrow_WhenVolumeNameIsValid()
    {
        // Arrange
        var appBuilder = DistributedApplication.CreateBuilder([]);
        var redisBuilder = appBuilder.AddRedis("redis");

        // Act
        var act = () => redisBuilder.WithDataVolume("redis-data");

        // Assert
        act.Should().NotThrow();
    }
}
