namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoClientFactoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteDumpsAsync_WhenConnectionStringIsNullOrWhiteSpace_ShouldThrow(string? connString)
    {
        var act = () => MongoClientFactory.ExecuteDumpsAsync(connString!, [], CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteDumpsAsync_WhenDumpsIsNull_ShouldThrow()
    {
        var act = () => MongoClientFactory.ExecuteDumpsAsync("mongodb://localhost:27017", null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
