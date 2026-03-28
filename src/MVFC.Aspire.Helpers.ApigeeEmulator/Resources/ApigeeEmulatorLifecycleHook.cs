namespace MVFC.Aspire.Helpers.ApigeeEmulator.Resources;

/// <summary>
/// Lifecycle hook that deploys the Apigee proxy bundle to the emulator
/// once the container reaches the Running state.
/// </summary>
public sealed class ApigeeEmulatorLifecycleHook(
    ResourceNotificationService notifications) : IDistributedApplicationEventingSubscriber
{
    private readonly IApigeeNotificationService _notifications = new DefaultApigeeNotificationService(notifications);
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Gets or sets the file system abstraction. Defaults to <see cref="DefaultApigeeFileSystem"/>.
    /// </summary>
    internal IApigeeFileSystem FileSystem { get; set; } = new DefaultApigeeFileSystem();

    /// <summary>
    /// Gets or sets the HTTP client factory.
    /// </summary>
    internal Func<int, HttpClient> HttpClientFactory { get; set; } = port =>
        new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };

    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventing);

        eventing.Subscribe<BeforeResourceStartedEvent>(OnBeforeResourceStartedAsync);
        eventing.Subscribe<AfterResourcesCreatedEvent>(OnAfterResourcesCreatedAsync);

        return Task.CompletedTask;
    }

    private async Task OnBeforeResourceStartedAsync(
        BeforeResourceStartedEvent @event,
        CancellationToken ct = default)
    {
        if (@event.Resource is not ApigeeEmulatorResource resource)
            return;

        await TryPrebuildBundleAsync(resource).ConfigureAwait(false);
    }

    private async Task OnAfterResourcesCreatedAsync(
        AfterResourcesCreatedEvent @event,
        CancellationToken ct = default)
    {
        foreach (var resource in @event.Model.Resources.OfType<ApigeeEmulatorResource>())
            await DeployAsync(resource, ct).ConfigureAwait(false);
    }

    internal async Task TryPrebuildBundleAsync(ApigeeEmulatorResource resource)
    {
        if (string.IsNullOrWhiteSpace(resource.WorkspacePath))
            return;

        try
        {
            var zipPath = GetBundlePath(resource);
            var targetServersJson = BuildTargetServersJsonOrNull(GetBackendAnnotation(resource));

            await BuildZipAsync(resource.WorkspacePath, zipPath, targetServersJson, resource.ApigeeEnvironment)
                .ConfigureAwait(false);

            resource.PrebuiltBundlePath = zipPath;
            resource.Annotations.Add(new ContainerMountAnnotation(
                source: zipPath,
                target: ApigeeEmulatorDefaults.CONTAINER_BUNDLE_PATH,
                type: ContainerMountType.BindMount,
                isReadOnly: true));
        }
        catch
        {
            // Portas dinâmicas em testes podem não estar resolvíveis aqui.
            // O DeployAsync reconstruirá o bundle após o container subir.
        }
    }

    internal async Task DeployAsync(ApigeeEmulatorResource resource, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resource.WorkspacePath) ||
            string.IsNullOrWhiteSpace(resource.HealthCheckPath))
        {
            return;
        }

        var backendAnnotation = GetBackendAnnotation(resource);
        await WaitForDependenciesAsync(resource, backendAnnotation, ct).ConfigureAwait(false);

        var controlPort = ResolveBackendPort(resource, ApigeeEmulatorResource.CONTROL_PORT_NAME);
        using var controlClient = HttpClientFactory(controlPort);

        await PollUntilReadyAsync(
            controlClient,
            ApigeeEmulatorDefaults.EMULATOR_READY_PATH,
            ApigeeEmulatorDefaults.EMULATOR_READY_MAX_RETRIES,
            ApigeeEmulatorDefaults.EMULATOR_READY_DELAY_SECONDS,
            ct).ConfigureAwait(false);

        var zipPath = await EnsureBundleAsync(resource, backendAnnotation).ConfigureAwait(false);
        await DeployZipAsync(controlClient, zipPath, resource.ApigeeEnvironment, ct).ConfigureAwait(false);

        var trafficPort = ResolveBackendPort(resource, ApigeeEmulatorResource.TRAFFIC_PORT_NAME);
        using var trafficClient = HttpClientFactory(trafficPort);

        await PollUntilReadyAsync(
            trafficClient,
            resource.HealthCheckPath,
            ApigeeEmulatorDefaults.PROXY_READY_MAX_RETRIES,
            ApigeeEmulatorDefaults.PROXY_READY_DELAY_SECONDS,
            ct).ConfigureAwait(false);
    }

    private async Task WaitForDependenciesAsync(
        ApigeeEmulatorResource resource,
        ApigeeTargetBackendAnnotation? backendAnnotation,
        CancellationToken ct)
    {
        await _notifications
            .WaitForResourceAsync(resource.Name, KnownResourceStates.Running, ct)
            .ConfigureAwait(false);

        if (backendAnnotation is not null)
        {
            await _notifications
                .WaitForResourceAsync(backendAnnotation.Backend.Name, KnownResourceStates.Running, ct)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reutiliza o bundle pré-construído se ainda for válido; caso contrário, reconstrói.
    /// </summary>
    internal async Task<string> EnsureBundleAsync(
        ApigeeEmulatorResource resource,
        ApigeeTargetBackendAnnotation? backendAnnotation)
    {
        if (FileSystem.FileExists(resource.PrebuiltBundlePath))
            return resource.PrebuiltBundlePath!;

        var zipPath = GetBundlePath(resource);
        var targetServersJson = BuildTargetServersJsonOrNull(backendAnnotation);

        await BuildZipAsync(resource.WorkspacePath!, zipPath, targetServersJson, resource.ApigeeEnvironment)
            .ConfigureAwait(false);

        return zipPath;
    }

    internal async Task BuildZipAsync(
        string workspacePath,
        string zipPath,
        string? targetServersJson,
        string environment)
    {
        if (FileSystem.FileExists(zipPath))
            FileSystem.FileDelete(zipPath);

        var tempDir = Path.Combine(Path.GetTempPath(), $"apigee-deploy-{Guid.NewGuid():N}");
        try
        {
            CopyDirectory(workspacePath, tempDir);

            if (targetServersJson is not null)
                await MergeTargetServersFile(tempDir, environment, targetServersJson).ConfigureAwait(false);

            await FileSystem.ZipCreateFromDirectoryAsync(tempDir, zipPath)
                .ConfigureAwait(false);
        }
        finally
        {
            if (FileSystem.DirectoryExists(tempDir))
                FileSystem.DirectoryDelete(tempDir, recursive: true);
        }
    }

    internal async Task MergeTargetServersFile(string tempDir, string environment, string incomingJson)
    {
        var tsPath = Path.Combine(tempDir, "src", "main", "apigee", "environments", environment, "targetservers.json");

        FileSystem.DirectoryCreateDirectory(Path.GetDirectoryName(tsPath)!);

        var merged = new List<JsonElement>();

        if (FileSystem.FileExists(tsPath))
        {
            var existingContent = FileSystem.FileReadAllText(tsPath);
            var existing = JsonSerializer.Deserialize<JsonElement>(existingContent);
            foreach (var item in existing.EnumerateArray()) merged.Add(item);
        }

        var incoming = JsonSerializer.Deserialize<JsonElement>(incomingJson);
        foreach (var item in incoming.EnumerateArray())
            merged.Add(item);

        await FileSystem.FileWriteAllTextAsync(tsPath, JsonSerializer.Serialize(merged, _jsonOptions)).ConfigureAwait(false);
    }

    internal void CopyDirectory(string source, string dest)
    {
        FileSystem.DirectoryCreateDirectory(dest);
        foreach (var file in FileSystem.DirectoryGetFiles(source, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(source, file);
            var destFile = Path.Combine(dest, rel);
            FileSystem.DirectoryCreateDirectory(Path.GetDirectoryName(destFile)!);
            FileSystem.FileCopy(file, destFile, overwrite: true);
        }
    }

    internal static int ResolveBackendPort(IResource resource, string endpointName)
    {
        var allEndpoints = resource.Annotations.OfType<EndpointAnnotation>().ToList();

        if (allEndpoints.Count == 1)
            return ExtractPort(allEndpoints[0], resource.Name);

        var partial = allEndpoints
            .FirstOrDefault(e => e.Name.Contains(endpointName, StringComparison.OrdinalIgnoreCase));

        return partial is not null
            ? ExtractPort(partial, resource.Name)
            : throw new InvalidOperationException($"[Apigee] Nenhum endpoint encontrado para o recurso '{resource.Name}' com nome '{endpointName}'.");
    }

    internal static int ExtractPort(EndpointAnnotation endpoint, string resourceName)
    {
        if (endpoint.AllocatedEndpoint is { Port: > 0 } allocated)
            return allocated.Port;

        if (endpoint.Port is int fixedPort)
            return fixedPort;

        throw new InvalidOperationException($"[Apigee] O endpoint '{endpoint.Name}' do recurso '{resourceName}' não tem uma porta configurada.");
    }

    private static async Task PollUntilReadyAsync(
        HttpClient client,
        string path,
        int maxRetries,
        int delaySeconds,
        CancellationToken ct)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var resp = await client.GetAsync(path, ct).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode) return;
            }
            catch { /* serviço ainda não está pronto */ }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timeout aguardando '{client.BaseAddress}{path}'");
    }

    internal async Task DeployZipAsync(
        HttpClient client,
        string zipPath,
        string environment,
        CancellationToken ct)
    {
        var fs = FileSystem.FileOpenRead(zipPath);

        await using (fs.ConfigureAwait(false))
        {
            using var content = new StreamContent(fs);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            var response = await client
                .PostAsync($"/v1/emulator/deploy?environment={Uri.EscapeDataString(environment)}", content, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                throw new InvalidOperationException($"[Apigee] Deploy falhou [{response.StatusCode}]: {body}");
            }
        }
    }

    private static string GetBundlePath(ApigeeEmulatorResource resource) =>
        Path.Combine(Path.GetTempPath(), $"apigee-{resource.Name}-bundle.zip");

    private static ApigeeTargetBackendAnnotation? GetBackendAnnotation(ApigeeEmulatorResource resource) =>
        resource.Annotations.OfType<ApigeeTargetBackendAnnotation>().FirstOrDefault();

    private static string? BuildTargetServersJsonOrNull(ApigeeTargetBackendAnnotation? annotation) =>
        annotation is not null
            ? BuildTargetServersJson(
                annotation.TargetServerName,
                ResolveBackendPort(annotation.Backend, annotation.EndpointName))
            : null;

    internal static string BuildTargetServersJson(string targetServerName, int port)
    {
        var servers = new[]
        {
        new
        {
            name = targetServerName,
            host = GetDockerHostAddress(),
            port,
            isEnabled = true
        }
    };
        return JsonSerializer.Serialize(servers, _jsonOptions);
    }

    internal static string GetDockerHostAddress()
    {
        if (!OperatingSystem.IsLinux())
            return ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST;

        // No Linux, enumera interfaces de bridge do Docker para obter o gateway real
        foreach (var iface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (iface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                continue;

            if (!iface.Name.StartsWith("br-", StringComparison.Ordinal) && iface.Name != "docker0")
                continue;

            var ip = iface.GetIPProperties().UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.Address.ToString();

            if (ip is not null) return ip;
        }

        return "172.17.0.1"; // fallback: gateway padrão da bridge docker0
    }
}
