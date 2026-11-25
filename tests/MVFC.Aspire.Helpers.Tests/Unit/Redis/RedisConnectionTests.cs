namespace MVFC.Aspire.Helpers.Tests.Unit.Redis;

public sealed class RedisConnectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("secret")]
    [InlineData("p @:/%,\"雪")]
    public async Task WithReference_ShouldPreserveFinalPasswordSeparately(string? password)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var redis = builder.AddRedis("cache");
        var consumer = builder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("consumer").WithReference(redis);
        var endpoint = redis.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 6379);

        if (password is not null)
            redis.WithPassword("old-secret").WithPassword(password);

        // Act
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, TestContext.Current.CancellationToken);
        var endpointValue = await ((IValueProvider)environment["ConnectionStrings__cache"]).GetValueAsync(TestContext.Current.CancellationToken);
        var passwordValue = await ((IValueProvider)environment["Redis__cache__Password"]).GetValueAsync(TestContext.Current.CancellationToken);
        var options = RedisConfigurationOptions.Parse(endpointValue!);
        options.Password = passwordValue;
        var args = new List<object>();
        var context = new CommandLineArgsCallbackContext(args, redis.Resource, TestContext.Current.CancellationToken);

        foreach (var callback in redis.Resource.Annotations.OfType<CommandLineArgsCallbackAnnotation>())
            await callback.Callback(context);

        // Assert
        endpointValue.Should().Be("localhost:6379");
        options.Password.Should().Be(password ?? string.Empty);
        redis.Resource.PasswordParameter.Secret.Should().BeTrue();
        args.OfType<string>().Count(value => value == "--requirepass").Should().Be(password is null ? 0 : 1);

        if (password is not null)
            (await ((IValueProvider)args.Last()).GetValueAsync(TestContext.Current.CancellationToken)).Should().Be(password);
    }

    [Fact]
    public async Task WithReference_WithMultipleResources_ShouldKeepPasswordsIndependent()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var first = builder.AddRedis("first").WithPassword("first-secret");
        var second = builder.AddRedis("second").WithPassword("second-secret");
        var consumer = builder.AddProject<MVFC_Aspire_Helpers_Playground_Api>("consumer").WithReference(first).WithReference(second);

        // Act
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, TestContext.Current.CancellationToken);
        var firstPassword = await ((IValueProvider)environment["Redis__first__Password"]).GetValueAsync(TestContext.Current.CancellationToken);
        var secondPassword = await ((IValueProvider)environment["Redis__second__Password"]).GetValueAsync(TestContext.Current.CancellationToken);

        // Assert
        firstPassword.Should().Be("first-secret");
        secondPassword.Should().Be("second-secret");
    }
}
