namespace MVFC.Aspire.Helpers.Tests.Unit.Mongo;

public sealed class MongoDumpProcessorTests
{
    [Fact]
    public async Task ProcessDumpsAsync_ShouldExecuteAllDumps()
    {
        var clientMock = new Mock<IMongoClient>();
        var dumpMock1 = new Mock<IMongoClassDump>();
        var dumpMock2 = new Mock<IMongoClassDump>();

        dumpMock1.Setup(d => d.ExecuteDumpAsync(clientMock.Object, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        dumpMock2.Setup(d => d.ExecuteDumpAsync(clientMock.Object, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await MongoDumpProcessor.ProcessDumpsAsync(clientMock.Object, [dumpMock1.Object, dumpMock2.Object], CancellationToken.None);

        dumpMock1.Verify(d => d.ExecuteDumpAsync(clientMock.Object, It.IsAny<CancellationToken>()), Times.Once);
        dumpMock2.Verify(d => d.ExecuteDumpAsync(clientMock.Object, It.IsAny<CancellationToken>()), Times.Once);
    }
}
