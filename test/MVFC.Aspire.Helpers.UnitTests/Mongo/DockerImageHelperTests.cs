namespace MVFC.Aspire.Helpers.UnitTests.Mongo;

public sealed class DockerImageHelperTests {
    [Fact]
    public void Build_WithValidValues_ShouldReturnCorrectFormat() {
        var result = DockerImageHelper.Build("mongo", "latest");

        result.Should().Be("mongo:latest");
    }

    [Fact]
    public void Build_WithCustomImage_ShouldReturnCorrectFormat() {
        var result = DockerImageHelper.Build("my-registry/mongo", "7.0");

        result.Should().Be("my-registry/mongo:7.0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_WithNullOrWhiteSpaceImage_ShouldThrow(string? image) {
        var act = () => DockerImageHelper.Build(image!, "latest");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_WithNullOrWhiteSpaceTag_ShouldThrow(string? tag) {
        var act = () => DockerImageHelper.Build("mongo", tag!);

        act.Should().Throw<ArgumentException>();
    }
}
