namespace MVFC.Aspire.Helpers.Tests.Unit.ApigeeEmulator;

public sealed class ApigeeEmulatorPlatformTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddApigeeEmulator_WithExplicitPlatform_ShouldConfigureExpectedRuntimeArgs(bool isLinux)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var args = new List<object>();
        var context = new ContainerRuntimeArgsCallbackContext(args, TestContext.Current.CancellationToken);

        // Act
        var apigee = builder.AddApigeeEmulator("apigee", controlPort: 7072, trafficPort: 8999, isLinux: isLinux);

        foreach (var callback in apigee.Resource.Annotations.OfType<ContainerRuntimeArgsCallbackAnnotation>())
            await callback.Callback(context);

        // Assert
        var expected = isLinux
            ? new[] { "--add-host", "host.docker.internal:host-gateway" }
            : Array.Empty<string>();

        args.Should().Equal(expected);
        apigee.Resource.Annotations.OfType<EndpointAnnotation>()
            .Single(endpoint => endpoint.Name == ApigeeEmulatorResource.CONTROL_PORT_NAME).Port.Should().Be(7072);
        apigee.Resource.Annotations.OfType<EndpointAnnotation>()
            .Single(endpoint => endpoint.Name == ApigeeEmulatorResource.TRAFFIC_PORT_NAME).Port.Should().Be(8999);
    }

    [Fact]
    public async Task AddApigeeEmulator_ShouldConfigureHostGatewayOnlyOnLinux()
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var args = new List<object>();
        var context = new ContainerRuntimeArgsCallbackContext(args, TestContext.Current.CancellationToken);

        // Act
        var apigee = builder.AddApigeeEmulator("apigee");

        foreach (var callback in apigee.Resource.Annotations.OfType<ContainerRuntimeArgsCallbackAnnotation>())
            await callback.Callback(context);

        // Assert
        var expected = OperatingSystem.IsLinux()
            ? new[] { "--add-host", "host.docker.internal:host-gateway" }
            : Array.Empty<string>();

        args.Should().Equal(expected);
    }
}
