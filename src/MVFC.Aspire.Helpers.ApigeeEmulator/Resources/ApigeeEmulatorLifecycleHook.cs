namespace MVFC.Aspire.Helpers.ApigeeEmulator.Resources;

/// <summary>
/// Lifecycle hook that deploys the Apigee proxy bundle to the emulator
/// once the container reaches the Running state.
/// </summary>
public sealed class ApigeeEmulatorLifecycleHook(
    ResourceNotificationService notifications) : IDistributedApplicationEventingSubscriber
{
    private readonly ResourceNotificationService _notifications = notifications;
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

    /// <summary>
    /// Subscribes to distributed application events to trigger Apigee emulator deployment logic.
    /// </summary>
    /// <param name="eventing">The eventing system to subscribe to.</param>
    /// <param name="executionContext">The execution context for the distributed application.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A completed task.</returns>
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventing);

        eventing.Subscribe<AfterResourcesCreatedEvent>(OnAfterResourcesCreatedAsync);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the event after resources are created, triggering deployment for each ApigeeEmulatorResource.
    /// </summary>
    /// <param name="event">The event containing the created resources.</param>
    /// <param name="ct">A cancellation token.</param>
    private async Task OnAfterResourcesCreatedAsync(
        AfterResourcesCreatedEvent @event,
        CancellationToken ct = default)
    {
        foreach (var resource in @event.Model.Resources.OfType<ApigeeEmulatorResource>())
            await DeployAsync(resource, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Deploys the Apigee proxy bundle to the emulator and waits for readiness.
    /// </summary>
    /// <param name="resource">The Apigee emulator resource to deploy to.</param>
    /// <param name="ct">A cancellation token.</param>
    internal async Task DeployAsync(ApigeeEmulatorResource resource, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resource.WorkspacePath) ||
            string.IsNullOrWhiteSpace(resource.HealthCheckPath))
        {
            return;
        }

        var backendAnnotations = GetBackendAnnotations(resource);
        await WaitForDependenciesAsync(resource, backendAnnotations, ct).ConfigureAwait(false);

        var controlPort = ResolveBackendPort(resource, ApigeeEmulatorResource.CONTROL_PORT_NAME);
        using var controlClient = HttpClientFactory(controlPort);

        await PollUntilReadyAsync(
            controlClient,
            ApigeeEmulatorDefaults.EMULATOR_READY_PATH,
            ApigeeEmulatorDefaults.EMULATOR_READY_MAX_RETRIES,
            ApigeeEmulatorDefaults.EMULATOR_READY_DELAY_SECONDS,
            ct).ConfigureAwait(false);

        var entries = ResolveEntries(backendAnnotations);
        var zipPath = await EnsureBundleAsync(resource, entries).ConfigureAwait(false);

        try
        {
            await DeployZipAsync(controlClient, zipPath, resource.ApigeeEnvironment, ct).ConfigureAwait(false);
        }
        finally
        {
            if (FileSystem.FileExists(zipPath))
                FileSystem.FileDelete(zipPath);
        }

        var trafficPort = ResolveBackendPort(resource, ApigeeEmulatorResource.TRAFFIC_PORT_NAME);
        using var trafficClient = HttpClientFactory(trafficPort);

        await PollUntilReadyAsync(
            trafficClient,
            resource.HealthCheckPath,
            ApigeeEmulatorDefaults.PROXY_READY_MAX_RETRIES,
            ApigeeEmulatorDefaults.PROXY_READY_DELAY_SECONDS,
            ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the Apigee emulator and its backend dependencies to reach the running state.
    /// </summary>
    /// <param name="resource">The Apigee emulator resource.</param>
    /// <param name="backendAnnotations">The backend annotation, if any.</param>
    /// <param name="ct">A cancellation token.</param>
    private async Task WaitForDependenciesAsync(
        ApigeeEmulatorResource resource,
        IReadOnlyList<ApigeeTargetBackendAnnotation> backendAnnotations,
        CancellationToken ct)
    {
        await _notifications
            .WaitForResourceAsync(resource.Name, KnownResourceStates.Running, ct)
            .ConfigureAwait(false);

        foreach (var annotation in backendAnnotations)
        {
            await _notifications
                .WaitForResourceAsync(annotation.Backend.Name, KnownResourceStates.Running, ct)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Ensures the Apigee proxy bundle zip file is created and ready for deployment.
    /// </summary>
    /// <param name="resource">The Apigee emulator resource.</param>
    /// <param name="entries">The list of target server entries.</param>
    /// <returns>The path to the created zip file.</returns>
    internal async Task<string> EnsureBundleAsync(
        ApigeeEmulatorResource resource,
        IReadOnlyList<TargetServerEntry> entries)
    {
        var zipPath = GetBundlePath(resource);
        var targetServersJson = BuildTargetServersJsonOrNull(entries);

        await BuildZipAsync(resource.WorkspacePath!, zipPath, targetServersJson, resource.ApigeeEnvironment)
            .ConfigureAwait(false);

        return zipPath;
    }

    /// <summary>
    /// Builds a zip archive of the Apigee proxy bundle, optionally merging target server configuration.
    /// </summary>
    /// <param name="workspacePath">The source workspace directory.</param>
    /// <param name="zipPath">The destination zip file path.</param>
    /// <param name="targetServersJson">Optional target servers JSON to merge.</param>
    /// <param name="environment">The Apigee environment name.</param>
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

    /// <summary>
    /// Merges the provided target servers JSON into the targetservers.json file for the specified environment.
    /// </summary>
    /// <param name="tempDir">The temporary directory containing the Apigee bundle.</param>
    /// <param name="environment">The Apigee environment name.</param>
    /// <param name="incomingJson">The incoming target servers JSON to merge.</param>
    internal async Task MergeTargetServersFile(string tempDir, string environment, string incomingJson)
    {
        var tsPath = Path.Combine(tempDir, "src", "main", "apigee", "environments", environment, "targetservers.json");

        FileSystem.DirectoryCreateDirectory(Path.GetDirectoryName(tsPath)!);

        var merged = new List<JsonElement>();

        if (FileSystem.FileExists(tsPath))
        {
            var existingContent = FileSystem.FileReadAllText(tsPath);
            var existing = JsonSerializer.Deserialize<JsonElement>(existingContent);

            if (existing.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException($"[Apigee] '{tsPath}' must be a JSON array. Got: {existing.ValueKind}.");

            foreach (var item in existing.EnumerateArray())
                merged.Add(item);
        }

        var incoming = JsonSerializer.Deserialize<JsonElement>(incomingJson);
        foreach (var item in incoming.EnumerateArray())
            merged.Add(item);

        await FileSystem.FileWriteAllTextAsync(tsPath, JsonSerializer.Serialize(merged, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Recursively copies all files and directories from the source to the destination directory.
    /// </summary>
    /// <param name="source">The source directory path.</param>
    /// <param name="dest">The destination directory path.</param>
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

    /// <summary>
    /// Resolves the port number for a given endpoint name on a resource.
    /// </summary>
    /// <param name="resource">The resource containing endpoint annotations.</param>
    /// <param name="endpointName">The name of the endpoint to resolve.</param>
    /// <returns>The resolved port number.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no matching endpoint is found.</exception>
    internal static int ResolveBackendPort(IResource resource, string endpointName)
    {
        var allEndpoints = resource.Annotations.OfType<EndpointAnnotation>().ToList();

        if (allEndpoints.Count == 1)
            return ExtractPort(allEndpoints[0], resource.Name);

        var partial = allEndpoints
            .FirstOrDefault(e => e.Name.Contains(endpointName, StringComparison.OrdinalIgnoreCase));

        return partial is not null
            ? ExtractPort(partial, resource.Name)
            : throw new InvalidOperationException($"[Apigee] No endpoint found for resource '{resource.Name}' with name '{endpointName}'.");
    }

    /// <summary>
    /// Extracts the port number from an endpoint annotation.
    /// </summary>
    /// <param name="endpoint">The endpoint annotation.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>The port number.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the endpoint does not have a configured port.</exception>
    internal static int ExtractPort(EndpointAnnotation endpoint, string resourceName)
    {
        if (endpoint.AllocatedEndpoint is { Port: > 0 } allocated)
            return allocated.Port;

        if (endpoint.Port is int fixedPort)
            return fixedPort;

        throw new InvalidOperationException($"[Apigee] The endpoint '{endpoint.Name}' of resource '{resourceName}' does not have a configured port.");
    }

    /// <summary>
    /// Polls the specified HTTP endpoint until it is ready or a timeout occurs.
    /// </summary>
    /// <param name="client">The HTTP client to use.</param>
    /// <param name="path">The path to poll.</param>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="delaySeconds">The delay in seconds between retries.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <exception cref="TimeoutException">Thrown if the endpoint does not become ready in time.</exception>
    private static async Task PollUntilReadyAsync(
        HttpClient client,
        string path,
        int maxRetries,
        int delaySeconds,
        CancellationToken ct)
    {
        var lastError = "No attempt made.";

        for (var i = 0; i < maxRetries; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                var resp = await client.GetAsync(path, timeoutCts.Token).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode) return;

                lastError = $"HTTP Status Code: {(int)resp.StatusCode} ({resp.StatusCode})";
            }
            catch (Exception ex)
            {
                lastError = $"Exception: {ex.Message}";
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timeout waiting for '{client.BaseAddress}{path}'. Last reported error: {lastError}");
    }

    /// <summary>
    /// Deploys the Apigee proxy bundle zip file to the emulator via HTTP.
    /// </summary>
    /// <param name="client">The HTTP client to use for deployment.</param>
    /// <param name="zipPath">The path to the zip file to deploy.</param>
    /// <param name="environment">The Apigee environment name.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown if the deployment fails.</exception>
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
                throw new InvalidOperationException($"[Apigee] Deploy failed [{response.StatusCode}]: {body}");
            }
        }
    }

    /// <summary>
    /// Returns the path to the Apigee proxy bundle zip file for the specified resource.
    /// </summary>
    /// <param name="resource">The Apigee emulator resource.</param>
    /// <returns>The path to the zip file.</returns>
    private static string GetBundlePath(ApigeeEmulatorResource resource) =>
        Path.Combine(Path.GetTempPath(), $"apigee-{resource.Name}-bundle.zip");

    /// <summary>
    /// Retrieves the backend annotations from the specified resource, if present.
    /// </summary>
    /// <param name="resource">The Apigee emulator resource.</param>
    /// <returns>A list of backend annotations, or an empty list if none are present.</returns>
    private static IReadOnlyList<ApigeeTargetBackendAnnotation> GetBackendAnnotations(ApigeeEmulatorResource resource) =>
        [.. resource.Annotations.OfType<ApigeeTargetBackendAnnotation>()];

    /// <summary>
    /// Builds the target servers JSON string from the provided entries, if any.
    /// </summary>
    /// <param name="entries">The list of target server entries.</param>
    /// <returns>The target servers JSON string, or <c>null</c> if the list is empty.</returns>
    internal static string? BuildTargetServersJsonOrNull(
        IReadOnlyList<TargetServerEntry> entries)
    {
        if (entries.Count == 0)
            return null;

        var servers = entries.Select(e => new
        {
            name = e.Name,
            host = e.Host,
            port = e.Port,
            isEnabled = true
        });

        return JsonSerializer.Serialize(servers, _jsonOptions);
    }

    /// <summary>
    /// Resolves the list of <see cref="TargetServerEntry"/> from the provided backend annotations.
    /// </summary>
    /// <param name="annotations">The backend annotations.</param>
    /// <returns>A list of resolved <see cref="TargetServerEntry"/> objects.</returns>
    private static IReadOnlyList<TargetServerEntry> ResolveEntries(
        IReadOnlyList<ApigeeTargetBackendAnnotation> annotations)
    {
        return [.. annotations.Select(a =>
        {
            var (host, port) = ResolveBackendEndpoint(a.Backend, a.EndpointName);
            return new TargetServerEntry(a.TargetServerName, host, port);
        })];
    }

    /// <summary>
    /// Resolves the host and port for a backend endpoint, supporting both container and project resources.
    /// </summary>
    /// <param name="resource">The backend resource.</param>
    /// <param name="endpointName">The endpoint name to resolve.</param>
    /// <returns>A tuple containing the host and port.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the endpoint cannot be resolved.</exception>
    internal static (string Host, int Port) ResolveBackendEndpoint(IResource resource, string endpointName)
    {
        var allEndpoints = resource.Annotations.OfType<EndpointAnnotation>().ToList();

        var endpoint = (allEndpoints.Count == 1
            ? allEndpoints[0]
            : allEndpoints.FirstOrDefault(e => e.Name.Contains(endpointName, StringComparison.OrdinalIgnoreCase))) ?? throw new InvalidOperationException($"[Apigee] No endpoint found for resource '{resource.Name}' with name '{endpointName}'.");

        // Checks if the backend is a container (e.g., WireMock, Keycloak, Redis)
        var isContainer = resource.Annotations.OfType<ContainerImageAnnotation>().Any();

        if (isContainer)
        {
            // Container-to-Container: Uses Aspire's internal network.
            // Docker DNS uses the resource name in lowercase and the image's internal port.
            var containerHost = resource.Name.ToLowerInvariant();
            var targetPort = endpoint.TargetPort ?? endpoint.Port ?? 80;
            return (containerHost, targetPort);
        }
        else
        {
            // Host-to-Container: If it's a .NET Project (builder.AddProject)
            var hostPort = endpoint.AllocatedEndpoint?.Port ?? endpoint.Port
                ?? throw new InvalidOperationException($"[Apigee] The endpoint '{endpoint.Name}' does not have a configured port.");

            return (ApigeeEmulatorDefaults.DOCKER_INTERNAL_HOST, hostPort);
        }
    }
}
