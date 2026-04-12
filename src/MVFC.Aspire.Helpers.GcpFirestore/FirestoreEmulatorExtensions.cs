namespace MVFC.Aspire.Helpers.GcpFirestore;

/// <summary>
/// Extension methods to register the Google Cloud Firestore emulator in Aspire.
/// </summary>
public static class FirestoreEmulatorExtensions
{
    /// <summary>
    /// Adds the Google Cloud Firestore emulator to the distributed application.
    /// Use fluent methods such as WithFirestoreConfigs and WithWaitTimeout to customize.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="port">The port to expose the emulator. Default is FirestoreDefaults.EMULATOR_PORT.</param>
    /// <returns>A resource builder for the Firestore emulator resource.</returns>
    public static IResourceBuilder<FirestoreEmulatorResource> AddGcpFirestore(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = FirestoreDefaults.EMULATOR_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        var resource = new FirestoreEmulatorResource(name);

        return builder.AddResource(resource)
                      .WithDockerImage(
                          image: FirestoreDefaults.EMULATOR_IMAGE,
                          tag: FirestoreDefaults.EMULATOR_IMAGE_TAG)
                      .WithFirestoreEndpoint(builder, port, name);
    }

    /// <summary>
    /// Adds multiple Firestore project configurations to the emulator.
    /// </summary>
    /// <param name="builder">The resource builder for the Firestore emulator.</param>
    /// <param name="configs">Firestore project configurations to add.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<FirestoreEmulatorResource> WithFirestoreConfigs(
        this IResourceBuilder<FirestoreEmulatorResource> builder,
        params FirestoreConfig[] configs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configs);

        builder.Resource.FirestoreConfigs = [.. builder.Resource.FirestoreConfigs, .. configs];
        return builder;
    }

    /// <summary>
    /// Replaces the Docker image used by the Firestore resource.
    /// </summary>
    /// <param name="builder">The resource builder for the Firestore emulator.</param>
    /// <param name="image">The Docker image name.</param>
    /// <param name="tag">The Docker image tag.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<FirestoreEmulatorResource> WithDockerImage(
        this IResourceBuilder<FirestoreEmulatorResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);

        return builder.WithImage(image)
                      .WithImageTag(tag);
    }

    /// <summary>
    /// Sets the maximum wait timeout for the emulator startup.
    /// </summary>
    /// <param name="builder">The resource builder for the Firestore emulator.</param>
    /// <param name="seconds">Timeout in seconds.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<FirestoreEmulatorResource> WithWaitTimeout(
        this IResourceBuilder<FirestoreEmulatorResource> builder,
        int seconds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThan(seconds, 0);

        builder.Resource.WaitTimeoutSeconds = seconds;
        return builder;
    }

    /// <summary>
    /// Adds a reference to the Firestore resource in the project, injecting
    /// FIRESTORE_EMULATOR_HOST, all ProjectIds (delimited), and registering
    /// collection provisioning via OnResourceReady.
    /// </summary>
    /// <param name="project">The project resource builder.</param>
    /// <param name="firestore">The Firestore emulator resource builder.</param>
    /// <returns>The project resource builder for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<FirestoreEmulatorResource> firestore)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(firestore);

        project.WithReference(source: firestore)
               .AddGCPProjectsIds(firestore.Resource.FirestoreConfigs)
               .WithEnvironment(
                   FirestoreDefaults.EMULATOR_HOST_ENV_VAR,
                   firestore.Resource.ConnectionStringExpression);

        RegisterFirestoreConfigurator(project, firestore);

        return project;
    }

    /// <summary>
    /// Injects all ProjectIds as a delimited environment variable — same pattern as PubSub.
    /// ASP.NET Core converts Firestore__ProjectIds → Firestore:ProjectIds in IConfiguration.
    /// </summary>
    /// <typeparam name="T">The resource type implementing IResourceWithEnvironment.</typeparam>
    /// <param name="resource">The resource builder.</param>
    /// <param name="firestoreConfigs">The Firestore project configurations.</param>
    /// <returns>The resource builder for chaining.</returns>
    private static IResourceBuilder<T> AddGCPProjectsIds<T>(
        this IResourceBuilder<T> resource,
        IReadOnlyList<FirestoreConfig> firestoreConfigs)
        where T : IResourceWithEnvironment
    {
        if (firestoreConfigs.Count == 0)
            return resource;

        var projectIds = string.Join(
            FirestoreDefaults.CREATION_DELIMITER,
            firestoreConfigs.Select(c => c.ProjectId));

        return resource.WithEnvironment(FirestoreDefaults.GCP_PROJECT_IDS_ENV_VAR, projectIds);
    }

    /// <summary>
    /// Registers the OnResourceReady callback responsible for provisioning collections
    /// in the emulator after startup, ensuring single execution via annotation.
    /// </summary>
    /// <param name="project">The project resource builder.</param>
    /// <param name="firestore">The Firestore emulator resource builder.</param>
    private static void RegisterFirestoreConfigurator(
        IResourceBuilder<ProjectResource> project,
        IResourceBuilder<FirestoreEmulatorResource> firestore)
    {
        if (firestore.Resource.FirestoreConfigs.Count == 0)
            return;

        if (firestore.Resource.TryGetAnnotationsOfType<FirestoreConfiguredAnnotation>(out _))
            return;

        firestore.WithAnnotation(new FirestoreConfiguredAnnotation());

        project.OnResourceReady(async (context, _, ct) =>
        {
            var port = context.GetEndpoint(FirestoreEmulatorResource.HTTP_ENDPOINT_NAME).Port;

            Environment.SetEnvironmentVariable(
                FirestoreDefaults.EMULATOR_HOST_ENV_VAR,
                $"localhost:{port.ToString(CultureInfo.InvariantCulture)}");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(firestore.Resource.WaitTimeoutSeconds));

            await FirestoreConfigurator.ConfigureAsync(
                firestore.Resource.FirestoreConfigs,
                port,
                cts.Token).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Configures the HTTP endpoint of the Firestore emulator, without proxy, with HTTP health check.
    /// </summary>
    /// <param name="resource">The Firestore emulator resource builder.</param>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="port">The port to expose the emulator.</param>
    /// <param name="name">The resource name.</param>
    /// <returns>The resource builder for chaining.</returns>
    private static IResourceBuilder<FirestoreEmulatorResource> WithFirestoreEndpoint(
        this IResourceBuilder<FirestoreEmulatorResource> resource,
        IDistributedApplicationBuilder builder,
        int port,
        string name)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        var healthCheckKey = builder.RegisterTcpHealthCheck(port, name);

        return resource.WithHttpEndpoint(
                           port: port,
                           targetPort: FirestoreDefaults.EMULATOR_PORT,
                           name: FirestoreEmulatorResource.HTTP_ENDPOINT_NAME,
                           isProxied: false)
                       .WithHealthCheck(healthCheckKey);
    }

    /// <summary>
    /// Registers a TCP health check for the Firestore emulator.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="port">The port to check.</param>
    /// <param name="name">The resource name.</param>
    /// <returns>The health check key.</returns>
    private static string RegisterTcpHealthCheck(
        this IDistributedApplicationBuilder builder,
        int port,
        string name)
    {
        var healthCheckKey = $"firestore_{name}";

        builder.Services
               .AddHealthChecks()
               .Add(new HealthCheckRegistration(
                   name: healthCheckKey,
                   factory: _ => new FirestoreTcpHealthCheck(port),
                   failureStatus: null,
                   tags: null));

        return healthCheckKey;
    }
}