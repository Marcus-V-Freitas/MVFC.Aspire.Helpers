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
        var path = await _sut.EnsureBundleAsync(resource, []);

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
    public void BuildTargetServersJson_SingleEntry_CorrectFormat()
    {
        // Arrange
        var entries = new List<TargetServerEntry>
        {
            new("my-ts", "test", 1234),
        };

        // Act
        var json = ApigeeEmulatorLifecycleHook.BuildTargetServersJsonOrNull(entries);

        // Assert
        json.Should().NotBeNull();
        json.Should().Contain("\"name\": \"my-ts\"");
        json.Should().Contain("\"port\": 1234");
        json.Should().Contain("\"isEnabled\": true");
    }

    [Fact]
    public void BuildTargetServersJson_MultipleEntries_ContainsAllAndIsArray()
    {
        // Arrange
        var entries = new List<TargetServerEntry>
        {
            new("origin",         "host-a", 5050),
            new("legacy-backend", "host-b", 5051),
        };

        // Act
        var json = ApigeeEmulatorLifecycleHook.BuildTargetServersJsonOrNull(entries);

        // Assert
        json.Should().NotBeNull();
        json!.TrimStart().Should().StartWith("[");
        json.Should().Contain("\"name\": \"origin\"");
        json.Should().Contain("\"port\": 5050");
        json.Should().Contain("\"name\": \"legacy-backend\"");
        json.Should().Contain("\"port\": 5051");
    }

    [Fact]
    public void BuildTargetServersJson_EmptyList_ReturnsNull()
    {
        // Arrange & Act
        var json = ApigeeEmulatorLifecycleHook.BuildTargetServersJsonOrNull([]);

        // Assert
        json.Should().BeNull();
    }

    [Fact]
    public void ResolveBackendEndpoint_SingleEndpoint_NonContainer_ReturnsDockerInternalHost()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("my-project");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "http", port: 5050)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be(ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST);
        port.Should().Be(5050);
    }

    [Fact]
    public void ResolveBackendEndpoint_SingleEndpoint_Container_ReturnsContainerHost()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("WireMock-Backend");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "http", port: 5050, targetPort: 8080),
            new ContainerImageAnnotation { Image = "wiremock/wiremock", Tag = "latest" }
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be("wiremock-backend");
        port.Should().Be(8080);
    }

    [Fact]
    public void ResolveBackendEndpoint_Container_UsesTargetPortWhenAvailable()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("Redis");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "tcp", port: 6379, targetPort: 6380),
            new ContainerImageAnnotation { Image = "redis", Tag = "7" }
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "tcp");

        // Assert
        host.Should().Be("redis");
        port.Should().Be(6380);
    }

    [Fact]
    public void ResolveBackendEndpoint_Container_FallsBackToPortWhenNoTargetPort()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("Backend");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "http", port: 3000),
            new ContainerImageAnnotation { Image = "my-app", Tag = "1.0" }
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be("backend");
        port.Should().Be(3000);
    }

    [Fact]
    public void ResolveBackendEndpoint_Container_FallsBackTo80WhenNoPortsDefined()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("Svc");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "http"),
            new ContainerImageAnnotation { Image = "app", Tag = "v1" }
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be("svc");
        port.Should().Be(80);
    }

    [Fact]
    public void ResolveBackendEndpoint_MultipleEndpoints_ReturnsMatchingByName()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("my-project");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "grpc", port: 9090),
            new EndpointAnnotation(ProtocolType.Tcp, name: "http", port: 5050)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be(ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST);
        port.Should().Be(5050);
    }

    [Fact]
    public void ResolveBackendEndpoint_MultipleEndpoints_ThrowsWhenNoneMatch()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("my-project");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "grpc", port: 9090),
            new EndpointAnnotation(ProtocolType.Tcp, name: "tcp", port: 5050)
        };
        resource.Annotations.Returns(annotations);

        // Act
        var act = () => ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No endpoint found*");
    }

    [Fact]
    public void ResolveBackendEndpoint_NonContainer_UsesAllocatedEndpointPort()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("my-project");
        var endpoint = new EndpointAnnotation(ProtocolType.Tcp, name: "http", port: 5050);
        endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, "localhost", 9999);
        var annotations = new ResourceAnnotationCollection { endpoint };
        resource.Annotations.Returns(annotations);

        // Act
        var (host, port) = ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        host.Should().Be(ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST);
        port.Should().Be(9999);
    }

    [Fact]
    public void ResolveBackendEndpoint_NonContainer_ThrowsWhenNoPortConfigured()
    {
        // Arrange
        var resource = Substitute.For<IResource>();
        resource.Name.Returns("my-project");
        var annotations = new ResourceAnnotationCollection
        {
            new EndpointAnnotation(ProtocolType.Tcp, name: "http")
        };
        resource.Annotations.Returns(annotations);

        // Act
        var act = () => ApigeeEmulatorLifecycleHook.ResolveBackendEndpoint(resource, "http");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not have a configured port*");
    }

    [Fact]
    public async Task BuildZipAsync_WithTargetServersJson_MergesFile()
    {
        // Arrange
        var workspacePath = Path.Combine("mock", "ws");
        var zipName = "bundle.zip";
        var targetServersJson = """[{"name":"backend","host":"localhost","port":5050,"isEnabled":true}]""";

        _fileSystem.FileExists(zipName).Returns(false);
        _fileSystem.DirectoryExists(Arg.Is<string>(s => s.StartsWith(Path.GetTempPath()))).Returns(true);
        _fileSystem.DirectoryGetFiles(workspacePath, "*", SearchOption.AllDirectories).Returns([]);

        // Act
        await _sut.BuildZipAsync(workspacePath, zipName, targetServersJson, "local");

        // Assert
        await _fileSystem.Received(1).FileWriteAllTextAsync(
            Arg.Is<string>(s => s.Contains("targetservers.json")),
            Arg.Is<string>(s => s.Contains("backend")));
        await _fileSystem.Received(1).ZipCreateFromDirectoryAsync(
            Arg.Is<string>(s => s.StartsWith(Path.GetTempPath())), zipName);
    }

    [Fact]
    public async Task DeployAsync_SkipsWhenHealthCheckPathIsWhiteSpace()
    {
        // Arrange
        var resource = new ApigeeEmulatorResource("test")
        {
            WorkspacePath = "some/path",
            HealthCheckPath = "   "
        };
        var httpCalled = false;
        _sut.HttpClientFactory = _ => { httpCalled = true; return null!; };

        // Act
        await _sut.DeployAsync(resource, TestContext.Current.CancellationToken);

        // Assert
        httpCalled.Should().BeFalse();
    }
}
