namespace MVFC.Aspire.Helpers.Tests.Unit.GcpBigQuery;

public sealed class BigQueryConfigTests
{
    [Fact]
    public void Constructor_ShouldSetProjectId()
    {
        var config = new BigQueryConfig("test-project");
        config.ProjectId.Should().Be("test-project");
        config.Dataset.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetProjectIdAndDataset()
    {
        var config = new BigQueryConfig("test-project", "test_dataset");
        config.ProjectId.Should().Be("test-project");
        config.Dataset.Should().Be("test_dataset");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenProjectIdIsInvalid(string? projectId)
    {
        var act = () => new BigQueryConfig(projectId!);
        act.Should().Throw<ArgumentException>();
    }
}
