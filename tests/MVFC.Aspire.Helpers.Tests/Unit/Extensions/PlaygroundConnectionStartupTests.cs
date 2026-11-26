namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class PlaygroundConnectionStartupTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ConnectAsync_WhenAlreadyCanceled_ShouldPreserveCancellation(bool redis)
    {
        // Arrange
        var builder = CreateBuilder(30);
        var token = new CancellationToken(true);

        // Act
        Func<Task> act = () => redis
            ? PlaygroundInstanceHelpers.CreateRedisAsync(builder, token)
            : PlaygroundInstanceHelpers.CreateRabbitAsync(builder, token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 0)]
    [InlineData(true, -1)]
    [InlineData(false, -1)]
    [InlineData(true, int.MaxValue)]
    [InlineData(false, int.MaxValue)]
    public async Task ConnectAsync_WhenBudgetIsInvalid_ShouldRejectConfiguration(bool redis, int seconds)
    {
        // Arrange
        var builder = CreateBuilder(seconds);

        // Act
        Func<Task> act = () => redis
            ? PlaygroundInstanceHelpers.CreateRedisAsync(builder, TestContext.Current.CancellationToken)
            : PlaygroundInstanceHelpers.CreateRabbitAsync(builder, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Configures unused endpoints for validation paths that must never connect.
    /// </summary>
    private static WebApplicationBuilder CreateBuilder(int seconds)
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:redis"] = "localhost:1",
            ["ConnectionStrings:rabbitmq"] = "amqp://test:pass@localhost:1/",
            ["Infrastructure:StartupTimeoutSeconds"] = seconds.ToString(CultureInfo.InvariantCulture),
        });
        return builder;
    }
}
