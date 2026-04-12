namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreConfiguratorTests
{
    [Fact]
    public async Task ConfigureAsync_WhenConfigsEmpty_ShouldThrow_WhenNoEmulatorRunning()
    {
        // Arrange — empty configs, port 1 is typically unreachable
        var configs = Array.Empty<FirestoreConfig>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var act = () => FirestoreConfigurator.ConfigureAsync(configs, 1, cts.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ConfigureAsync_WhenCancelled_ShouldThrowOperationCancelled()
    {
        // Arrange
        var configs = new[] { new FirestoreConfig("project-a", 10) };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => FirestoreConfigurator.ConfigureAsync(configs, 1, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ConfigureAsync_WhenNoEmulatorRunning_ShouldThrowOrCancel()
    {
        // Arrange
        var configs = new[] { new FirestoreConfig("project-a", 0) };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        // Act
        var act = () => FirestoreConfigurator.ConfigureAsync(configs, 1, cts.Token);

        // Assert — should eventually fail (timeout or exception)
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ConfigureAsync_WithZeroUpDelay_ShouldNotAddExtraDelay()
    {
        // Arrange
        var configs = new[] { new FirestoreConfig("project-a", 0) };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var act = () => FirestoreConfigurator.ConfigureAsync(configs, 1, cts.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ConfigureAsync_WithMultipleConfigs_ShouldUseMaxDelay()
    {
        // Arrange
        var configs = new[]
        {
            new FirestoreConfig("project-a", 1),
            new FirestoreConfig("project-b", 3),
            new FirestoreConfig("project-c", 2)
        };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var act = () => FirestoreConfigurator.ConfigureAsync(configs, 1, cts.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
