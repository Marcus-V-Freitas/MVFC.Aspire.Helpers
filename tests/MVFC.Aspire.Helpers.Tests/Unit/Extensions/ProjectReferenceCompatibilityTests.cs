namespace MVFC.Aspire.Helpers.Tests.Unit.Extensions;

public sealed class ProjectReferenceCompatibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithReference_ShouldInjectStorageSettingsForBothProjectModels(bool legacy)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var storage = builder.AddCloudStorage("storage");
        var endpoint = storage.Resource.Annotations.OfType<EndpointAnnotation>().Single();
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 4443);
        IResourceBuilder<IResourceWithEnvironment> consumer = legacy
            ? builder.AddTestProject("consumer").WithReference(storage)
            : builder.AddDotnetProject("consumer", new TestProjectMetadata().ProjectPath).WithReference(storage);
        var token = TestContext.Current.CancellationToken;

        // Act
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, token);
        var connection = await ((IValueProvider)environment["ConnectionStrings__storage"]).GetValueAsync(token);
        var emulator = await ((IValueProvider)environment["STORAGE_EMULATOR_HOST"]).GetValueAsync(token);

        // Assert
        connection.Should().Be("http://localhost:4443/storage/v1/");
        emulator.Should().Be(connection);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithReference_ShouldInjectSmtpSettingsForBothProjectModels(bool legacy)
    {
        // Arrange
        var builder = DistributedApplication.CreateBuilder([]);
        var mailpit = builder.AddMailpit("mailpit");
        var endpoint = mailpit.Resource.Annotations.OfType<EndpointAnnotation>().Single(annotation => annotation.Name == "smtp");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 1025);
        IResourceBuilder<IResourceWithEnvironment> consumer = legacy
            ? builder.AddTestProject("consumer").WithReference(mailpit)
            : builder.AddDotnetProject("consumer", new TestProjectMetadata().ProjectPath).WithReference(mailpit);

        // Act
        var environment = await builder.GetConfiguredEnvironmentAsync(consumer.Resource, TestContext.Current.CancellationToken);
        var connection = await ((IValueProvider)environment["ConnectionStrings__mailpit"]).GetValueAsync(TestContext.Current.CancellationToken);

        // Assert
        connection.Should().Be("smtp://localhost:1025");
    }
}
