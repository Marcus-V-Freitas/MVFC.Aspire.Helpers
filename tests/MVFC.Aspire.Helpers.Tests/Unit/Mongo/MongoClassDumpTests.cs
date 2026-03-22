namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public class TestDoc { public string Id { get; set; } = string.Empty; }

public sealed class MongoClassDumpTests
{
    [Fact]
    public async Task ExecuteDumpAsync_WhenClientIsNull_ShouldThrow()
    {
        var dump = new MongoClassDump<TestDoc>("db", "coll", 10, new Faker<TestDoc>());
        var act = () => dump.ExecuteDumpAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteDumpAsync_WhenQuantityIsZero_ShouldNotInsert()
    {
        var dbMock = new Mock<IMongoDatabase>();
        var collMock = new Mock<IMongoCollection<TestDoc>>();
        var clientMock = new Mock<IMongoClient>();

        clientMock.Setup(c => c.GetDatabase("db", null)).Returns(dbMock.Object);
        dbMock.Setup(d => d.GetCollection<TestDoc>("coll", null)).Returns(collMock.Object);

        var dump = new MongoClassDump<TestDoc>("db", "coll", 0, new Faker<TestDoc>());
        await dump.ExecuteDumpAsync(clientMock.Object, CancellationToken.None);

        collMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<TestDoc>>(), It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteDumpAsync_WhenQuantityIsGreaterThanZero_ShouldInsert()
    {
        var dbMock = new Mock<IMongoDatabase>();
        var collMock = new Mock<IMongoCollection<TestDoc>>();
        var clientMock = new Mock<IMongoClient>();

        clientMock.Setup(c => c.GetDatabase("db", null)).Returns(dbMock.Object);
        dbMock.Setup(d => d.GetCollection<TestDoc>("coll", null)).Returns(collMock.Object);

        var dump = new MongoClassDump<TestDoc>("db", "coll", 2, new Faker<TestDoc>().RuleFor(x => x.Id, f => f.Random.String()));
        await dump.ExecuteDumpAsync(clientMock.Object, CancellationToken.None);

        collMock.Verify(c => c.InsertManyAsync(It.Is<IEnumerable<TestDoc>>(x => x.Count() == 2), null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
