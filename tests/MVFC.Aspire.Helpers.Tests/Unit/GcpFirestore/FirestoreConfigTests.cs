namespace MVFC.Aspire.Helpers.Tests.Unit.GcpFirestore;

public sealed class FirestoreConfigTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenProjectIdIsNullOrWhitespace(string? projectId)
    {
        var act = () => new FirestoreConfig(projectId!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var config = new FirestoreConfig("my-project");
        config.ProjectId.Should().Be("my-project");
    }
}
