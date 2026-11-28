namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreConfigTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenProjectIdIsNullOrWhitespace(string? projectId)
    {
        // Arrange & Act
        var act = () => new FirestoreConfig(projectId!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var config = new FirestoreConfig("my-project");

        // Assert
        config.ProjectId.Should().Be("my-project");
    }
}
