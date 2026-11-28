namespace MVFC.Aspire.Helpers.Tests.Unit.GcpBigQuery;

public sealed class BigQueryConfigTests
{
    [Fact]
    public void Constructor_ShouldSetProjectId()
    {
        // Arrange & Act
        var config = new BigQueryConfig("test-project");

        // Assert
        config.ProjectId.Should().Be("test-project");
        config.Dataset.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetProjectIdAndDataset()
    {
        // Arrange & Act
        var config = new BigQueryConfig("test-project", "test_dataset");

        // Assert
        config.ProjectId.Should().Be("test-project");
        config.Dataset.Should().Be("test_dataset");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenProjectIdIsInvalid(string? projectId)
    {
        // Arrange & Act
        var act = () => new BigQueryConfig(projectId!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
