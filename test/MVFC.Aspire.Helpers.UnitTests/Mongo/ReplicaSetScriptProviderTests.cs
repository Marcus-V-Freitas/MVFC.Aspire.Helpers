using MVFC.Aspire.Helpers.Mongo.Helpers;

namespace MVFC.Aspire.Helpers.UnitTests.Mongo;

public class ReplicaSetScriptProviderTests {
    [Fact]
    public void GetInitScript_ShouldReturnNonEmptyString() {
        var script = ReplicaSetScriptProvider.GetInitScript();

        script.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetInitScript_ShouldContainReplicaSetInit() {
        var script = ReplicaSetScriptProvider.GetInitScript();

        script.Should().Contain("rs.initiate");
    }

    [Fact]
    public void GetInitScript_ShouldContainReplicaSetId() {
        var script = ReplicaSetScriptProvider.GetInitScript();

        script.Should().Contain("rs0");
    }

    [Fact]
    public void GetInitScript_ShouldContainHost() {
        var script = ReplicaSetScriptProvider.GetInitScript();

        script.Should().Contain("localhost:27017");
    }

    [Fact]
    public void GetInitScript_ShouldContainStatusCheck() {
        var script = ReplicaSetScriptProvider.GetInitScript();

        script.Should().Contain("rs.status");
    }
}
