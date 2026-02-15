using Bogus;
using MVFC.Aspire.Helpers.Mongo.Models;

namespace MVFC.Aspire.Helpers.UnitTests.Mongo;

public class MongoClassDumpTests {
    private class TestDocument {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties() {
        var faker = new Faker<TestDocument>()
            .RuleFor(x => x.Name, f => f.Name.FirstName())
            .RuleFor(x => x.Age, f => f.Random.Int(18, 99));

        var dump = new MongoClassDump<TestDocument>("testdb", "users", 100, faker);

        dump.DatabaseName.Should().Be("testdb");
        dump.CollectionName.Should().Be("users");
        dump.Quantity.Should().Be(100);
        dump.Faker.Should().BeSameAs(faker);
    }

    [Fact]
    public void IMongoClassDump_ShouldExposeCorrectProperties() {
        var faker = new Faker<TestDocument>();
        IMongoClassDump dump = new MongoClassDump<TestDocument>("db", "col", 50, faker);

        dump.DatabaseName.Should().Be("db");
        dump.CollectionName.Should().Be("col");
        dump.Quantity.Should().Be(50);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly() {
        var faker = new Faker<TestDocument>();
        var dump1 = new MongoClassDump<TestDocument>("db", "col", 10, faker);
        var dump2 = new MongoClassDump<TestDocument>("db", "col", 10, faker);

        dump1.Should().Be(dump2);
    }
}
