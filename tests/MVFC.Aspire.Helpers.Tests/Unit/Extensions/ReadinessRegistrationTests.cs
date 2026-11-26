namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class ReadinessRegistrationTests
{
    [Fact]
    public void WithHealthChecks_ShouldBeOptInIdempotentAndIsolated()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var first = builder.AddMongoReplicaSet("first");
        var second = builder.AddMongoReplicaSet("second");
        var redis = builder.AddRedis("cache");

        first.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().BeEmpty();
        redis.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().BeEmpty();

        // Act
        first.WithPrimaryHealthCheck().WithPrimaryHealthCheck();
        second.WithPrimaryHealthCheck();
        redis.WithTcpHealthCheck().WithTcpHealthCheck();
        using var services = builder.Services.BuildServiceProvider();
        var registrations = services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations;

        // Assert
        first.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().ContainSingle();
        second.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().ContainSingle();
        redis.Resource.Annotations.OfType<HealthCheckAnnotation>().Should().ContainSingle();
        registrations.Should().HaveCount(3);
        registrations.Select(r => r.Name).Should().OnlyHaveUniqueItems();
        registrations.Should().OnlyContain(r => r.Timeout == TimeSpan.FromSeconds(3));
        registrations.Select(r => r.Factory(services)).Should().ContainItemsAssignableTo<IHealthCheck>();
    }

    [Fact]
    public void WithHealthChecks_WhenBuilderIsNull_ShouldRejectConfiguration()
    {
        // Arrange
        IResourceBuilder<MongoReplicaSetResource> mongo = null!;
        IResourceBuilder<RedisResource> redis = null!;

        // Act
        var configureMongo = () => mongo.WithPrimaryHealthCheck();
        var configureRedis = () => redis.WithTcpHealthCheck();

        // Assert
        configureMongo.Should().Throw<ArgumentNullException>();
        configureRedis.Should().Throw<ArgumentNullException>();
    }
}
