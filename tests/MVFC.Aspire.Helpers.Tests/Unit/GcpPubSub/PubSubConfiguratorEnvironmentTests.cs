namespace MVFC.Aspire.Helpers.Tests.Unit.GcpPubSub;

[Collection("PubSub emulator environment")]
public sealed class PubSubConfiguratorEnvironmentTests
{
    [Fact]
    public async Task ModifyPushEndpoint_WhenEmulatorIsSelectedFromEnvironment_ShouldHonorRpcCancellation()
    {
        // Arrange
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var endpoint = $"127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture)}";
        var previous = Environment.GetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
        var message = new MessageConfig("topic", "sub");

        try
        {
            Environment.SetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR, endpoint);

            // Act
            var operation = PubSubConfigurator.ModifyPushEndpoint("project", message, "http://localhost:1234", cancellation.Token);
            using var connection = await listener.AcceptTcpClientAsync(deadline.Token);
            cancellation.Cancel();
            var act = () => operation;

            // Assert
            await act.Should().ThrowAsync<Exception>().Where(exception =>
                exception is OperationCanceledException || exception is RpcException && ((RpcException)exception).StatusCode == StatusCode.Cancelled);
            deadline.IsCancellationRequested.Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(PubSubDefaults.EMULATOR_HOST_ENV_VAR, previous);
        }
    }
}
