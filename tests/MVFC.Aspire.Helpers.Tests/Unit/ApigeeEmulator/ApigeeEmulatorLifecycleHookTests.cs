namespace MVFC.Aspire.Helpers.Tests.Unit.ApigeeEmulator;

public sealed class ApigeeEmulatorLifecycleHookTests : IDisposable
{
    private readonly ResourceNotificationService _notifications = AppHelper.GetResourceNotificationService();
    private readonly IApigeeFileSystem _fileSystem = Substitute.For<IApigeeFileSystem>();
    private readonly ApigeeEmulatorLifecycleHook _sut;

    public ApigeeEmulatorLifecycleHookTests() =>
        _sut = new(_notifications)
        {
            FileSystem = _fileSystem
        };

    public void Dispose() =>
        _notifications.Dispose();

    [Fact]
    public async Task SubscribeAsync_ThrowsIfNullEventing()
    {
        Func<Task> act = () => _sut.SubscribeAsync(null!, null!, TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeployAsync_SkipsIfInvalidResource()
    {
        var resource = new ApigeeEmulatorResource("test") { WorkspacePath = null };
        var httpCalled = false;
        _sut.HttpClientFactory = _ => { httpCalled = true; return null!; };

        await _sut.DeployAsync(resource, TestContext.Current.CancellationToken);

        httpCalled.Should().BeFalse("o método deve retornar antes de qualquer chamada HTTP");
    }

    [Fact]
    public async Task EnsureBundleAsync_RebuildsIfBundleMissing()
    {
        var resource = new ApigeeEmulatorResource("apigee")
        {
            WorkspacePath = @"C:\src",
            ApigeeEnvironment = "test"
        };
        _fileSystem.FileExists(Arg.Any<string?>()).Returns(false);
        _fileSystem.DirectoryGetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SearchOption>())
                   .Returns([]);
        _fileSystem.ZipCreateFromDirectoryAsync(Arg.Any<string>(), Arg.Any<string>())
                   .Returns(Task.CompletedTask);

        var path = await _sut.EnsureBundleAsync(resource, null);

        path.Should().NotBeNullOrWhiteSpace();
        await _fileSystem.Received(1).ZipCreateFromDirectoryAsync(Arg.Any<string>(), path);
    }

    [Fact]
    public async Task BuildZipAsync_DeletesExistingAndCreatesNew()
    {
        _fileSystem.FileExists("old.zip").Returns(true);
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);
        _fileSystem.DirectoryGetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SearchOption>())
                   .Returns([]);

        await _sut.BuildZipAsync(@"C:\ws", "old.zip", null, "test");

        _fileSystem.Received(1).FileDelete("old.zip");
        await _fileSystem.Received(1).ZipCreateFromDirectoryAsync(Arg.Any<string>(), "old.zip");
        _fileSystem.Received(1).DirectoryDelete(Arg.Any<string>(), true);
    }

    [Fact]
    public async Task MergeTargetServersFile_CreatesNewFile()
    {
        _fileSystem.FileExists(Arg.Any<string>()).Returns(false);

        await _sut.MergeTargetServersFile(@"C:\temp", "test", """[{"name":"backend"}]""");

        await _fileSystem.Received(1).FileWriteAllTextAsync(
            Arg.Any<string>(),
            Arg.Is<string>(s => s.Contains("backend")));
    }

    [Fact]
    public async Task MergeTargetServersFile_MergesWithExisting()
    {
        _fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        _fileSystem.FileReadAllText(Arg.Any<string>()).Returns("""[{"name":"existing"}]""");

        await _sut.MergeTargetServersFile(@"C:\temp", "test", """[{"name":"new"}]""");

        await _fileSystem.Received(1).FileWriteAllTextAsync(
            Arg.Any<string>(),
            Arg.Is<string>(s => s.Contains("existing") && s.Contains("new")));
    }

    [Fact]
    public void CopyDirectory_CopiesFilesRecursively()
    {
        _fileSystem.DirectoryGetFiles(@"C:\src", "*", SearchOption.AllDirectories)
                   .Returns([@"C:\src\file1.txt", @"C:\src\sub\file2.txt"]);

        _sut.CopyDirectory(@"C:\src", @"C:\dest");

        _fileSystem.Received(2).FileCopy(Arg.Any<string>(), Arg.Any<string>(), true);
    }

    [Fact]
    public void ResolveBackendPort_ReturnsSingleEndpoint()
    {
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, port: 1234)
        };
        resource.Annotations.Returns(annotations);

        ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "any").Should().Be(1234);
    }

    [Fact]
    public void ResolveBackendPort_ReturnsNamedEndpoint()
    {
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "other", port: 1111),
            new EndpointAnnotation(ProtocolType.Tcp, name: "target", port: 2222)
        };
        resource.Annotations.Returns(annotations);

        ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "target").Should().Be(2222);
    }

    [Fact]
    public void ResolveBackendPort_ThrowsIfNotFound()
    {
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "one", port: 1),
            new EndpointAnnotation(ProtocolType.Tcp, name: "two", port: 2)
        };
        resource.Annotations.Returns(annotations);

        var act = () => ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "missing");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExtractPort_ReturnsAllocatedPort()
    {
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 5000);

        ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res").Should().Be(5000);
    }

    [Fact]
    public void ExtractPort_ReturnsFixedPort()
    {
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep", port: 9090);

        ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res").Should().Be(9090);
    }

    [Fact]
    public void ExtractPort_ThrowsIfNoPort()
    {
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep");

        var act = () => ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task DeployZipAsync_ThrowsOnFailure()
    {
        var client = new HttpClient(new FakeStatusHandler(HttpStatusCode.BadRequest, "Error body"))
        { BaseAddress = new Uri("http://localhost") };
        _fileSystem.FileOpenRead(Arg.Any<string>()).Returns(new MemoryStream());

        var act = () => _sut.DeployZipAsync(client, "zip", "env", TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("BadRequest").And.Contain("Error body");
    }

    [Fact]
    public async Task DeployZipAsync_SucceedsOnOk()
    {
        var client = new HttpClient(new FakeOkHandler()) { BaseAddress = new Uri("http://localhost") };
        _fileSystem.FileOpenRead(Arg.Any<string>()).Returns(new MemoryStream());

        Func<Task> act = () => _sut.DeployZipAsync(client, "zip", "env", TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void BuildTargetServersJson_CorrectFormat()
    {
        var json = ApigeeEmulatorLifecycleHook.BuildTargetServersJson("my-ts", "test", 1234);

        json.Should().Contain("\"name\": \"my-ts\"");
        json.Should().Contain("\"port\": 1234");
        json.Should().Contain("\"isEnabled\": true");
    }
}
