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
        // Arrange
        Func<Task> act = () => _sut.SubscribeAsync(null!, null!, TestContext.Current.CancellationToken);

        // Act & Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeployAsync_SkipsIfInvalidResource()
    {
        // Arrange
        var resource = new ApigeeEmulatorResource("test") { WorkspacePath = null };
        var httpCalled = false;
        _sut.HttpClientFactory = _ => { httpCalled = true; return null!; };

        // Act
        await _sut.DeployAsync(resource, TestContext.Current.CancellationToken);

        // Assert
        httpCalled.Should().BeFalse("o método deve retornar antes de qualquer chamada HTTP");
    }

    [Fact]
    public async Task EnsureBundleAsync_RebuildsIfBundleMissing()
    {
        // Arrange
        var workspacePath = Path.Combine("mock", "src");
        var resource = new ApigeeEmulatorResource("apigee")
        {
            WorkspacePath = workspacePath,
            ApigeeEnvironment = "test"
        };
        var expectedZipPath = Path.Combine(Path.GetTempPath(), "apigee-apigee-bundle.zip");
        
        _fileSystem.FileExists(expectedZipPath).Returns(false);
        _fileSystem.DirectoryGetFiles(workspacePath, "*", SearchOption.AllDirectories).Returns([]);
        _fileSystem.ZipCreateFromDirectoryAsync(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath())), expectedZipPath)
                   .Returns(Task.CompletedTask);

        // Act
        var path = await _sut.EnsureBundleAsync(resource, null);

        // Assert
        path.Should().Be(expectedZipPath);
        await _fileSystem.Received(1).ZipCreateFromDirectoryAsync(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath())), expectedZipPath);
    }

    [Fact]
    public async Task BuildZipAsync_DeletesExistingAndCreatesNew()
    {
        // Arrange
        var workspacePath = Path.Combine("mock", "ws");
        var zipName = "old.zip";
        
        _fileSystem.FileExists(zipName).Returns(true);
        _fileSystem.DirectoryExists(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath()))).Returns(true);
        _fileSystem.DirectoryGetFiles(workspacePath, "*", SearchOption.AllDirectories).Returns([]);

        // Act
        await _sut.BuildZipAsync(workspacePath, zipName, null, "test");

        // Assert
        _fileSystem.Received(1).FileDelete(zipName);
        await _fileSystem.Received(1).ZipCreateFromDirectoryAsync(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath())), zipName);
        _fileSystem.Received(1).DirectoryDelete(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath())), true);
    }

    [Fact]
    public async Task MergeTargetServersFile_CreatesNewFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "tempDir");
        var expectedTsPath = Path.Combine(tempDir, "src", "main", "apigee", "environments", "test", "targetservers.json");
        _fileSystem.FileExists(expectedTsPath).Returns(false);

        // Act
        await _sut.MergeTargetServersFile(tempDir, "test", """[{"name":"backend"}]""");

        // Assert
        await _fileSystem.Received(1).FileWriteAllTextAsync(
            expectedTsPath,
            Arg.Is<string>(s => s.Contains("backend")));
    }

    [Fact]
    public async Task MergeTargetServersFile_MergesWithExisting()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "tempDir");
        var expectedTsPath = Path.Combine(tempDir, "src", "main", "apigee", "environments", "test", "targetservers.json");
        _fileSystem.FileExists(expectedTsPath).Returns(true);
        _fileSystem.FileReadAllText(expectedTsPath).Returns("""[{"name":"existing"}]""");

        // Act
        await _sut.MergeTargetServersFile(tempDir, "test", """[{"name":"new"}]""");

        // Assert
        await _fileSystem.Received(1).FileWriteAllTextAsync(
            expectedTsPath,
            Arg.Is<string>(s => s.Contains("existing") && s.Contains("new")));
    }

    [Fact]
    public async Task MergeTargetServersFile_ThrowsIfExistingIsNotArray()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "tempDir");
        var expectedTsPath = Path.Combine(tempDir, "src", "main", "apigee", "environments", "test", "targetservers.json");
        _fileSystem.FileExists(expectedTsPath).Returns(true);
        _fileSystem.FileReadAllText(expectedTsPath).Returns("""{"name":"not_array"}""");

        // Act
        var act = () => _sut.MergeTargetServersFile(tempDir, "test", """[{"name":"new"}]""");

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("must be a JSON array");
    }

    [Fact]
    public void CopyDirectory_CopiesFilesRecursively()
    {
        // Arrange
        var srcDir = Path.Combine(Path.GetTempPath(), "src");
        var destDir = Path.Combine(Path.GetTempPath(), "dest");
        var file1 = Path.Combine(srcDir, "file1.txt");
        var file2 = Path.Combine(srcDir, "sub", "file2.txt");

        _fileSystem.DirectoryGetFiles(srcDir, "*", SearchOption.AllDirectories)
                   .Returns([file1, file2]);

        // Act
        _sut.CopyDirectory(srcDir, destDir);

        // Assert
        _fileSystem.Received(1).FileCopy(file1, Path.Combine(destDir, "file1.txt"), true);
        _fileSystem.Received(1).FileCopy(file2, Path.Combine(destDir, "sub", "file2.txt"), true);
    }

    [Fact]
    public void ResolveBackendPort_ReturnsSingleEndpoint()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, port: 1234)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var port = ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "any");

        // Assert
        port.Should().Be(1234);
    }

    [Fact]
    public void ResolveBackendPort_ReturnsNamedEndpoint()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "other", port: 1111),
            new EndpointAnnotation(ProtocolType.Tcp, name: "target", port: 2222)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var port = ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "target");

        // Assert
        port.Should().Be(2222);
    }

    [Fact]
    public void ResolveBackendPort_ThrowsIfNotFound()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("res");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "one", port: 1),
            new EndpointAnnotation(ProtocolType.Tcp, name: "two", port: 2)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var act = () => ApigeeEmulatorLifecycleHook.ResolveBackendPort(resource, "missing");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExtractPort_ReturnsAllocatedPort()
    {
        // Arrange
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep");
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 5000);

        // Act
        var port = ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res");

        // Assert
        port.Should().Be(5000);
    }

    [Fact]
    public void ExtractPort_ReturnsFixedPort()
    {
        // Arrange
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep", port: 9090);

        // Act
        var port = ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res");

        // Assert
        port.Should().Be(9090);
    }

    [Fact]
    public void ExtractPort_ThrowsIfNoPort()
    {
        // Arrange
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "ep");

        // Act
        var act = () => ApigeeEmulatorLifecycleHook.ExtractPort(endpoint, "res");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task DeployZipAsync_ThrowsOnFailure()
    {
        // Arrange
        var client = new HttpClient(new FakeStatusHandler(HttpStatusCode.BadRequest, "Error body"))
        { BaseAddress = new Uri("http://localhost") };
        _fileSystem.FileOpenRead("dummy.zip").Returns(new MemoryStream());

        // Act
        var act = () => _sut.DeployZipAsync(client, "dummy.zip", "test_env", TestContext.Current.CancellationToken);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("BadRequest").And.Contain("Error body");
    }

    [Fact]
    public async Task DeployZipAsync_SucceedsOnOk()
    {
        // Arrange
        var client = new HttpClient(new FakeOkHandler()) { BaseAddress = new Uri("http://localhost") };
        _fileSystem.FileOpenRead("valid.zip").Returns(new MemoryStream());

        // Act
        Func<Task> act = () => _sut.DeployZipAsync(client, "valid.zip", "test_env", TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void BuildTargetServersJson_CorrectFormat()
    {
        // Arrange & Act
        var json = ApigeeEmulatorLifecycleHook.BuildTargetServersJson("my-ts", "test", 1234);

        // Assert
        json.Should().Contain("\"name\": \"my-ts\"");
        json.Should().Contain("\"port\": 1234");
        json.Should().Contain("\"isEnabled\": true");
    }
}
