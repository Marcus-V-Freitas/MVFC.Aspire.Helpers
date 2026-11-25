namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoUpsertDumpTests
{
    [Fact]
    public async Task ExecuteDumpAsync_ShouldUseSerializedIdsAndFullReplacements()
    {
        // Arrange
        var client = Substitute.For<IMongoClient>();
        var database = Substitute.For<IMongoDatabase>();
        var collection = Substitute.For<IMongoCollection<BsonDocument>>();
        client.GetDatabase("seeds").Returns(database);
        database.GetCollection<BsonDocument>("items").Returns(collection);
        var faker = new Faker<BsonDocument>().CustomInstantiator(_ => new BsonDocument { ["_id"] = "stable", ["value"] = "updated" });
        var dump = new MongoUpsertDump<BsonDocument>("seeds", "items", 1, faker);
        IEnumerable<WriteModel<BsonDocument>>? captured = null;
        collection.BulkWriteAsync(Arg.Any<IEnumerable<WriteModel<BsonDocument>>>(), Arg.Any<BulkWriteOptions>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<IEnumerable<WriteModel<BsonDocument>>>();
                return (BulkWriteResult<BsonDocument>)null!;
            });

        // Act
        await dump.ExecuteDumpAsync(client, TestContext.Current.CancellationToken);

        // Assert
        var write = captured.Should().ContainSingle().Subject.Should().BeOfType<ReplaceOneModel<BsonDocument>>().Subject;
        write.IsUpsert.Should().BeTrue();
        write.Replacement["_id"].AsString.Should().Be("stable");
        write.Replacement["value"].AsString.Should().Be("updated");
        var filter = write.Filter.Render(new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry));
        filter["_id"].AsString.Should().Be("stable");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteDumpAsync_WhenIdentitiesInvalid_ShouldRejectBeforeWriting(bool duplicate)
    {
        // Arrange
        var client = Substitute.For<IMongoClient>();
        var faker = new Faker<BsonDocument>().CustomInstantiator(_ => duplicate ? new BsonDocument("_id", "same") : new BsonDocument("value", 1));
        var dump = new MongoUpsertDump<BsonDocument>("seeds", "items", 2, faker);

        // Act
        var act = () => dump.ExecuteDumpAsync(client, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        client.DidNotReceive().GetDatabase(Arg.Any<string>(), Arg.Any<MongoDatabaseSettings>());
    }

    [Fact]
    public async Task ExecuteDumpAsync_WhenEmpty_ShouldNotWrite()
    {
        // Arrange
        var client = Substitute.For<IMongoClient>();
        var dump = new MongoUpsertDump<BsonDocument>("seeds", "items", 0, new Faker<BsonDocument>());

        // Act
        await dump.ExecuteDumpAsync(client, TestContext.Current.CancellationToken);

        // Assert
        client.DidNotReceive().GetDatabase(Arg.Any<string>(), Arg.Any<MongoDatabaseSettings>());
    }

    [Fact]
    public async Task ExecuteDumpAsync_WhenCanceled_ShouldNotWrite()
    {
        // Arrange
        var client = Substitute.For<IMongoClient>();
        var dump = new MongoUpsertDump<BsonDocument>("seeds", "items", 1, new Faker<BsonDocument>());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => dump.ExecuteDumpAsync(client, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        client.DidNotReceive().GetDatabase(Arg.Any<string>(), Arg.Any<MongoDatabaseSettings>());
    }
}
