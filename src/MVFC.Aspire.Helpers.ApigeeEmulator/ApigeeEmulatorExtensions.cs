namespace MVFC.Aspire.Helpers.ApigeeEmulator;

/// <summary>
/// Extension methods to simplify Apigee Emulator integration with .NET Aspire,
/// including resource registration, workspace and environment configuration.
/// </summary>
public static class ApigeeEmulatorExtensions
{
    /// <summary>
    /// Adds the Apigee Emulator to the distributed application with default settings.
    /// Use fluent methods like <see cref="WithWorkspace"/>, <see cref="WithEnvironment"/>,
    /// and <see cref="WithBackend{T}"/> to customize.
    /// </summary>
    /// <param name="builder">Distributed application builder.</param>
    /// <param name="name">Logical resource name for the Apigee Emulator.</param>
    /// <param name="controlPort">Host-side port for the control API. Default: 7071.</param>
    /// <param name="trafficPort">Host-side port for the traffic gateway. Default: 8998.</param>
    /// <returns>A resource builder for the <see cref="ApigeeEmulatorResource"/>.</returns>
    public static IResourceBuilder<ApigeeEmulatorResource> AddApigeeEmulator(
        this IDistributedApplicationBuilder builder,
        string name,
        int controlPort = ApigeeEmulatorDefaults.DEFAULT_CONTROL_PORT,
        int trafficPort = ApigeeEmulatorDefaults.DEFAULT_TRAFFIC_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var resource = new ApigeeEmulatorResource(name);

        builder.Services.TryAddEventingSubscriber<ApigeeEmulatorLifecycleHook>();

        var resourceBuilder = builder.AddResource(resource)
            .WithDockerImage(
                image: ApigeeEmulatorDefaults.DEFAULT_IMAGE,
                tag: ApigeeEmulatorDefaults.DEFAULT_IMAGE_TAG)
            .WithApigeeEndpoints(controlPort, trafficPort);

        if (OperatingSystem.IsLinux())
        {
            return resourceBuilder.WithContainerRuntimeArgs("--add-host", "host.docker.internal:host-gateway");
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Sets the local workspace path containing the Apigee proxy bundle (apiproxy root).
    /// </summary>
    /// <param name="builder">Builder for the Apigee Emulator resource.</param>
    /// <param name="workspacePath">Absolute or relative path to the proxy workspace.</param>
    /// <param name="healthCheckPath">HTTP path used for health checks.</param>
    /// <returns>The same builder, for fluent chaining.</returns>
    public static IResourceBuilder<ApigeeEmulatorResource> WithWorkspace(
        this IResourceBuilder<ApigeeEmulatorResource> builder,
        string workspacePath,
        string healthCheckPath)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath);
        ArgumentNullException.ThrowIfNull(healthCheckPath);

        builder.Resource.WorkspacePath = workspacePath;
        builder.Resource.HealthCheckPath = healthCheckPath;

        return builder;
    }

    /// <summary>
    /// Sets the Apigee environment name used during proxy deploy.
    /// Default: <c>"local"</c>.
    /// </summary>
    /// <param name="builder">Builder for the Apigee Emulator resource.</param>
    /// <param name="environment">Apigee environment name (e.g., "local", "dev").</param>
    /// <returns>The same builder, for fluent chaining.</returns>
    public static IResourceBuilder<ApigeeEmulatorResource> WithEnvironment(
        this IResourceBuilder<ApigeeEmulatorResource> builder,
        string environment)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        builder.Resource.ApigeeEnvironment = environment;
        return builder;
    }

    /// <summary>
    /// Overrides the Docker image used by the Apigee Emulator resource.
    /// </summary>
    /// <param name="builder">Builder for the Apigee Emulator resource.</param>
    /// <param name="image">Docker image name.</param>
    /// <param name="tag">Docker image tag.</param>
    /// <returns>The same builder, for fluent chaining.</returns>
    public static IResourceBuilder<ApigeeEmulatorResource> WithDockerImage(
        this IResourceBuilder<ApigeeEmulatorResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return builder.WithAnnotation(new ContainerImageAnnotation
        {
            Registry = ApigeeEmulatorDefaults.DEFAULT_IMAGE_REGISTRY,
            Image = image,
            Tag = tag,
        });
    }

    /// <summary>
    /// Configures an Aspire backend as a TargetServer for the Apigee proxy.
    /// The backend must expose an endpoint with a fixed port (e.g., <c>.WithHttpEndpoint(port: 5050)</c>).
    /// From the emulator perspective (inside Docker), the backend is accessed via <c>host.docker.internal</c>.
    /// </summary>
    /// <typeparam name="T">Type of the backend resource.</typeparam>
    /// <param name="builder">Builder for the Apigee Emulator resource.</param>
    /// <param name="backend">Builder for the backend resource that exposes endpoints.</param>
    /// <param name="targetServerName">TargetServer name referenced in targets/default.xml.</param>
    /// <param name="endpointName">Name of the endpoint exposed by the backend resource. Default: <c>"http"</c>.</param>
    /// <returns>The same builder, for fluent chaining.</returns>
    public static IResourceBuilder<ApigeeEmulatorResource> WithBackend<T>(
        this IResourceBuilder<ApigeeEmulatorResource> builder,
        IResourceBuilder<T> backend,
        string targetServerName,
        string endpointName = "http")
            where T : IResourceWithEndpoints, IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(backend);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(targetServerName);

        // Ensure the backend endpoint bypasses the Aspire proxy to allow direct Docker access
        backend.WithEndpoint(endpointName, e => e.IsProxied = false);

        var endpoint = backend.GetEndpoint(endpointName);

        // Force ASP.NET Core apps to bind to 0.0.0.0 instead of localhost, which is required
        // when IsProxied = false for Docker containers to reach it via host.docker.internal
        backend.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://0.0.0.0:{endpoint.Property(EndpointProperty.Port)}"));

        builder.Resource.Annotations.Add(new ApigeeTargetBackendAnnotation
        {
            Backend = backend.Resource,
            EndpointName = endpointName,
            TargetServerName = targetServerName,
        });

        return builder;
    }

    /// <summary>
    /// Configures the control and traffic HTTP endpoints for the Apigee Emulator.
    /// </summary>
    private static IResourceBuilder<ApigeeEmulatorResource> WithApigeeEndpoints(
        this IResourceBuilder<ApigeeEmulatorResource> builder,
        int controlPort,
        int trafficPort)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(controlPort, IPEndPoint.MinPort);
        ArgumentOutOfRangeException.ThrowIfLessThan(trafficPort, IPEndPoint.MinPort);

        return builder
            .WithHttpEndpoint(
                port: controlPort,
                targetPort: ApigeeEmulatorDefaults.CONTROL_TARGET_PORT,
                name: ApigeeEmulatorResource.CONTROL_PORT_NAME,
                isProxied: false)
            .WithHttpEndpoint(
                port: trafficPort,
                targetPort: ApigeeEmulatorDefaults.TRAFFIC_TARGET_PORT,
                name: ApigeeEmulatorResource.TRAFFIC_PORT_NAME,
                isProxied: false)
            .WithHttpHealthCheck(
                endpointName: ApigeeEmulatorResource.CONTROL_PORT_NAME,
                path: ApigeeEmulatorDefaults.EMULATOR_READY_PATH);
    }
}
