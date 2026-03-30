namespace MVFC.Aspire.Helpers.Tests.Unit.ApigeeEmulator;

public sealed class WithBackendTests
{
    [Fact]
    public void WithBackend_FirstCall_AddsAnnotation()
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();

        // Act
        builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-a");

        // Assert
        builder.Resource.Annotations
            .OfType<ApigeeTargetBackendAnnotation>()
            .Should().ContainSingle(a => a.TargetServerName == "server-a");
    }

    [Fact]
    public void WithBackend_DifferentNames_AddsBothAnnotations()
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();

        // Act
        builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-a");
        builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-b");

        // Assert
        builder.Resource.Annotations
            .OfType<ApigeeTargetBackendAnnotation>()
            .Should().HaveCount(2);
    }

    [Fact]
    public void WithBackend_DuplicateName_ThrowsBeforeAddingAnnotation()
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();
        builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-a");

        // Act
        var act = () => builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-a");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*server-a*");
        builder.Resource.Annotations.OfType<ApigeeTargetBackendAnnotation>().Should().HaveCount(1);
    }

    [Fact]
    public void WithBackend_DuplicateName_IsCaseInsensitive()
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();
        builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "MyServer");

        // Act
        var act = () => builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "myserver");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WithBackend_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IResourceBuilder<ApigeeEmulatorResource> builder = null!;

        // Act
        var act = () => builder.WithBackend(ApigeeEmulatorTestHelpers.CreateBackend(), "server-a");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void WithBackend_NullBackend_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();

        // Act
        var act = () => builder.WithBackend<FakeBackend>(null!, "server-a");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("backend");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void WithBackend_NullOrWhitespaceTargetServerName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var builder = ApigeeEmulatorTestHelpers.CreateBuilder();
        var backend = ApigeeEmulatorTestHelpers.CreateBackend();

        // Act
        var act = () => builder.WithBackend(backend, name!);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("targetServerName");
    }
}
