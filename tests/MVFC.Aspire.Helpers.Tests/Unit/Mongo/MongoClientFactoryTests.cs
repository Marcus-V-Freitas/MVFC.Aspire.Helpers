namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoClientFactoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteDumpsAsync_WhenConnectionStringIsNullOrWhiteSpace_ShouldThrow(string? connString)
    {
        // Arrange

        // Act
        var act = () => MongoClientFactory.ExecuteDumpsAsync(connString!, [], CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteDumpsAsync_WhenDumpsIsNull_ShouldThrow()
    {
        // Arrange

        // Act
        var act = () => MongoClientFactory.ExecuteDumpsAsync("mongodb://localhost:27017", null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("same", "same", true)]
    [InlineData("Secret", "secret", false)]
    public async Task ExecuteDumpsAsync_ShouldRespectPasswordIdentity(string first, string second, bool shared)
    {
        // Arrange
        var database = $"cache_{Guid.NewGuid():N}";
        var clients = new List<IMongoClient>();
        var dump = Substitute.For<IMongoClassDump>();
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                clients.Add(call.Arg<IMongoClient>());
                return Task.CompletedTask;
            });

        // Act
        await MongoClientFactory.ExecuteDumpsAsync($"mongodb://user:{first}@localhost/{database}", [dump], TestContext.Current.CancellationToken);
        await MongoClientFactory.ExecuteDumpsAsync($"mongodb://user:{second}@localhost/{database}", [dump], TestContext.Current.CancellationToken);

        // Assert
        ReferenceEquals(clients[0], clients[1]).Should().Be(shared);
    }

    [Fact]
    public async Task ExecuteDumpsAsync_ShouldRespectDatabaseCase()
    {
        // Arrange
        var database = $"cache_{Guid.NewGuid():N}";
        var clients = new List<IMongoClient>();
        var dump = Substitute.For<IMongoClassDump>();
        dump.ExecuteDumpAsync(Arg.Any<IMongoClient>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                clients.Add(call.Arg<IMongoClient>());
                return Task.CompletedTask;
            });

        // Act
        await MongoClientFactory.ExecuteDumpsAsync($"mongodb://localhost/{database}", [dump], TestContext.Current.CancellationToken);
        await MongoClientFactory.ExecuteDumpsAsync($"mongodb://localhost/{database.ToUpperInvariant()}", [dump], TestContext.Current.CancellationToken);

        // Assert
        clients[0].Should().NotBeSameAs(clients[1]);
    }
}
