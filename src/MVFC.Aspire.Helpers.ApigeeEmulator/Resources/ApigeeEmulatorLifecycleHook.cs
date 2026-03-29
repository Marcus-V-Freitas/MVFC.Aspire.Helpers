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
        new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };

    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventing);

        eventing.Subscribe<AfterResourcesCreatedEvent>(OnAfterResourcesCreatedAsync);

        return Task.CompletedTask;
    }

    private async Task OnAfterResourcesCreatedAsync(
        AfterResourcesCreatedEvent @event,
        CancellationToken ct = default)
    {
        foreach (var resource in @event.Model.Resources.OfType<ApigeeEmulatorResource>())
            await DeployAsync(resource, ct).ConfigureAwait(false);
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

    internal async Task<string> EnsureBundleAsync(
        ApigeeEmulatorResource resource,
        ApigeeTargetBackendAnnotation? backendAnnotation)
    {
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
        var lastError = "Nenhuma tentativa realizada.";

        for (var i = 0; i < maxRetries; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // Evita hangs infinitos no CI

                var resp = await client.GetAsync(path, timeoutCts.Token).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode) return;

                // Se chegou aqui, não é sucesso (ex: 404, 500, 503)
                lastError = $"Status Code HTTP: {(int)resp.StatusCode} ({resp.StatusCode})";
            }
            catch (Exception ex)
            {
                lastError = $"Exceção: {ex.Message}";
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timeout aguardando '{client.BaseAddress}{path}'. Último erro reportado: {lastError}");
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

    private static string? BuildTargetServersJsonOrNull(ApigeeTargetBackendAnnotation? annotation)
    {
        if (annotation is null) return null;

        var (host, port) = ResolveBackendEndpoint(annotation.Backend, annotation.EndpointName);

        return BuildTargetServersJson(annotation.TargetServerName, host, port);
    }

    internal static string BuildTargetServersJson(string targetServerName, string host, int port)
    {
        var servers = new[]
        {
            new
            {
                name = targetServerName,
                host = host, // Agora dinâmico (resolve o nome do container ou o IP do host)
                port = port, // Usa a TargetPort para containers
                isEnabled = true
            }
        };
        return JsonSerializer.Serialize(servers, _jsonOptions);
    }

    // Substitui o antigo ResolveBackendPort e ExtractPort
    internal static (string Host, int Port) ResolveBackendEndpoint(IResource resource, string endpointName)
    {
        var allEndpoints = resource.Annotations.OfType<EndpointAnnotation>().ToList();

        var endpoint = (allEndpoints.Count == 1
            ? allEndpoints[0]
            : allEndpoints.FirstOrDefault(e => e.Name.Contains(endpointName, StringComparison.OrdinalIgnoreCase))) ?? throw new InvalidOperationException($"[Apigee] Nenhum endpoint encontrado para o recurso '{resource.Name}' com nome '{endpointName}'.");

        // Verifica se o backend é um container (ex: WireMock, Keycloak, Redis)
        var isContainer = resource.Annotations.OfType<ContainerImageAnnotation>().Any();

        if (isContainer)
        {
            // Container-to-Container: Usa a rede interna do Aspire.
            // O DNS do Docker usa o nome do recurso em minúsculas e a porta interna da imagem.
            var containerHost = resource.Name.ToLowerInvariant();
            var targetPort = endpoint.TargetPort ?? endpoint.Port ?? 80;
            return (containerHost, targetPort);
        }
        else
        {
            // Host-to-Container: Se for um Projeto .NET (builder.AddProject)
            var hostPort = endpoint.AllocatedEndpoint?.Port ?? endpoint.Port
                ?? throw new InvalidOperationException($"[Apigee] O endpoint '{endpoint.Name}' não tem porta configurada.");

            return (ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST, hostPort);
        }
    }
}
