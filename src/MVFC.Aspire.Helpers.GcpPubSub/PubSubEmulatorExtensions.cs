namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Provides extension methods to configure, initialize, and integrate the Google Pub/Sub emulator
/// in distributed applications.
/// </summary>
public static class PubSubEmulatorExtensions
{
    /// <summary>
    /// Sets the shared endpoint resolution and subscription configuration deadline, in seconds.
    /// This does not change the emulator image initialization timeout.
    /// </summary>
    public static IResourceBuilder<PubSubEmulatorResource> WithConfigurationTimeout(
        this IResourceBuilder<PubSubEmulatorResource> builder,
        int seconds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seconds, PubSubDefaults.MAX_CONFIGURATION_TIMEOUT_SECONDS);

        builder.Resource.ConfigurationTimeoutSeconds = seconds;
        return builder;
    }

    /// <summary>
    /// Adds the Google Pub/Sub emulator to the distributed application.
    /// Use fluent methods such as WithPubSubConfigs and WithWaitTimeout to customize.
    /// </summary>
    public static IResourceBuilder<PubSubEmulatorResource> AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = PubSubDefaults.EMULATOR_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var resource = new PubSubEmulatorResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: PubSubDefaults.EMULATOR_IMAGE,
                tag: PubSubDefaults.EMULATOR_IMAGE_TAG)
            .WithEnvironment(ctx => ApplyEmulatorEnvironment(ctx, resource))
            .WithPubSubEndpoint(port);
    }

    /// <summary>
    /// Replaces the Docker image used by the Pub/Sub emulator.
    /// </summary>
    public static IResourceBuilder<PubSubEmulatorResource> WithDockerImage(
        this IResourceBuilder<PubSubEmulatorResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Adds multiple Pub/Sub project configurations to the emulator.
    /// </summary>
    public static IResourceBuilder<PubSubEmulatorResource> WithPubSubConfigs(
        this IResourceBuilder<PubSubEmulatorResource> builder,
        params PubSubConfig[] configs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configs);

        builder.Resource.PubSubConfigs = [.. builder.Resource.PubSubConfigs, .. configs];
        RegisterPubSubConfigurator(builder);
        return builder;
    }

    /// <summary>
    /// Sets the wait timeout for emulator initialization.
    /// </summary>
    public static IResourceBuilder<PubSubEmulatorResource> WithWaitTimeout(
        this IResourceBuilder<PubSubEmulatorResource> builder,
        int seconds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThan(seconds, 0);

        builder.Resource.WaitTimeoutSeconds = seconds;
        return builder;
    }

    /// <summary>
    /// Adds a reference to the PubSub resource in the project, configuring the
    /// PUBSUB_EMULATOR_HOST environment variable, and automatic subscription settings configuration
    /// ensuring global single registration via IResourceAnnotation. Add WaitFor explicitly when needed.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<PubSubEmulatorResource> pubSub)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(pubSub);

        project.WithReference(source: pubSub)
               .AddGCPProjectsIds(pubSub.Resource)
               .WithEnvironment(PubSubDefaults.EMULATOR_HOST_ENV_VAR, pubSub.Resource.ConnectionStringExpression);

        RegisterPubSubConfigurator(pubSub);

        return project;
    }

    /// <summary>
    /// Applies the environment variables required by the Pub/Sub emulator: wait timeout and
    /// the list of projects/configs in the format expected by the image.
    /// </summary>
    private static void ApplyEmulatorEnvironment(
        EnvironmentCallbackContext ctx,
        PubSubEmulatorResource resource)
    {
        ctx.EnvironmentVariables[PubSubDefaults.EMULATOR_WAIT_TIMEOUT_ENV_VAR] = resource.WaitTimeoutSeconds.ToString(CultureInfo.InvariantCulture);

        var projectNumber = 0;
        foreach (var pubSubConfig in resource.PubSubConfigs)
            ctx.EnvironmentVariables[$"{PubSubDefaults.PROJECT_ENV_VAR_PREFIX}{(++projectNumber).ToString(CultureInfo.InvariantCulture)}"] = PubSubProjectBuilder.Build(pubSubConfig);
    }

    /// <summary>
    /// Registers the OnResourceReady callback responsible for updating subscription settings
    /// in the Pub/Sub emulator after initialization, ensuring single execution via annotation.
    /// </summary>
    private static void RegisterPubSubConfigurator(IResourceBuilder<PubSubEmulatorResource> pubSub)
    {
        // Ensures the global emulator configuration callback is added only once
        if (pubSub.Resource.TryGetAnnotationsOfType<PubSubConfiguredAnnotation>(out _))
            return;

        pubSub.WithAnnotation(new PubSubConfiguredAnnotation());

        pubSub.OnResourceReady(async (context, readyEvent, ct) =>
        {
            var logger = readyEvent.Services.GetRequiredService<ResourceLoggerService>().GetLogger(context);
            await PubSubEmulatorInitializer.InitializeAsync(pubSub.Resource, logger, ct).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Adds the GCP project IDs environment variable to the specified resource.
    /// </summary>
    private static IResourceBuilder<T> AddGCPProjectsIds<T>(
        this IResourceBuilder<T> resource,
        PubSubEmulatorResource emulator)
        where T : IResourceWithEnvironment
    {
        return resource.WithEnvironment(ctx =>
        {
            if (emulator.PubSubConfigs.Count > 0)
                ctx.EnvironmentVariables[PubSubDefaults.GCP_PROJECT_IDS_ENV_VAR] = string.Join(
                    PubSubDefaults.CREATION_DELIMITER, emulator.PubSubConfigs.Select(c => c.ProjectId));
        });
    }

    /// <summary>
    /// Configures the HTTP endpoint of the Pub/Sub emulator, with a proxy and an HTTP health check.
    /// </summary>
    private static IResourceBuilder<PubSubEmulatorResource> WithPubSubEndpoint(
        this IResourceBuilder<PubSubEmulatorResource> resource,
        int port)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource.WithHttpEndpoint(
            port: port,
            targetPort: PubSubDefaults.EMULATOR_PORT,
            name: PubSubEmulatorResource.HTTP_ENDPOINT_NAME,
            isProxied: true)
            .WithHttpHealthCheck("/");
    }
}
