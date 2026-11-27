namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Provides extension methods to configure and integrate the Google Pub/Sub administrative UI
/// in distributed applications.
/// </summary>
public static class PubSubUIExtensions
{
    /// <summary>
    /// Adds the standalone Google Pub/Sub emulator admin UI to the application.
    /// Use WithReference(emulator) to associate the UI with an existing emulator.
    /// </summary>
    public static IResourceBuilder<PubSubUIResource> AddGcpPubSubUI(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = PubSubDefaults.UI_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var resource = new PubSubUIResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: PubSubDefaults.UI_IMAGE,
                tag: PubSubDefaults.UI_IMAGE_TAG)
            .WithImageSHA256(PubSubDefaults.UI_IMAGE_SHA256)
            .WithPubSubUiEndpoint(port);
    }

    /// <summary>
    /// Replaces the Docker image used by the Pub/Sub UI.
    /// </summary>
    public static IResourceBuilder<PubSubUIResource> WithDockerImage(
        this IResourceBuilder<PubSubUIResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Associates the Pub/Sub admin UI with a specific emulator, establishing
    /// the emulator host and project ID environment variables. Add WaitFor explicitly when needed.
    /// </summary>
    public static IResourceBuilder<PubSubUIResource> WithReference(
        this IResourceBuilder<PubSubUIResource> ui,
        IResourceBuilder<PubSubEmulatorResource> emulator)
    {
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(emulator);

        ui.WithEnvironment(PubSubDefaults.EMULATOR_HOST_ENV_VAR, emulator.Resource.ConnectionStringExpression);
        ApplyProjectIdsEnv(ui, emulator.Resource);
        return ui;
    }

    /// <summary>
    /// Applies the Pub/Sub project IDs environment variable to the UI, if configurations exist.
    /// </summary>
    private static void ApplyProjectIdsEnv(
        IResourceBuilder<PubSubUIResource> ui,
        PubSubEmulatorResource emulator)
    {
        ui.WithEnvironment(ctx =>
        {
            if (emulator.PubSubConfigs.Count > 0)
                ctx.EnvironmentVariables[PubSubDefaults.GCP_PROJECT_IDS_ENV_VAR] = string.Join(
                    PubSubDefaults.CREATION_DELIMITER, emulator.PubSubConfigs.Select(c => c.ProjectId));
        });
    }

    /// <summary>
    /// Configures the HTTP endpoint of the Pub/Sub UI, with a proxy and an HTTP health check.
    /// </summary>
    private static IResourceBuilder<PubSubUIResource> WithPubSubUiEndpoint(
        this IResourceBuilder<PubSubUIResource> resource,
        int port)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource.WithHttpEndpoint(
                port: port,
                targetPort: PubSubDefaults.UI_PORT,
                name: PubSubUIResource.HttpEndpointName,
                isProxied: true)
            .WithHttpHealthCheck("/");
    }
}
