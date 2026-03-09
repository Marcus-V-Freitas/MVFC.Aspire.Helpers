namespace MVFC.Aspire.Helpers.RabbitMQ;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of the RabbitMQ resource
/// in distributed applications using Aspire.
/// </summary>
public static class RabbitMQExtensions
{
    /// <summary>
    /// Adds a RabbitMQ resource to the distributed application with default settings.
    /// Use fluent methods such as WithCredentials, WithExchanges, WithQueues, WithDataVolume to customize.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> AddRabbitMQ(
        this IDistributedApplicationBuilder builder,
        string name,
        int amqpPort = RabbitMQDefaults.DEFAULT_AMQP_PORT,
        int httpPort = RabbitMQDefaults.DEFAULT_MANAGEMENT_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new RabbitMQResource(name);

        var rb = builder.AddResource(resource)
            .WithDockerImage(
                image: RabbitMQDefaults.DEFAULT_RABBIT_MQ_IMAGE,
                tag: RabbitMQDefaults.DEFAULT_RABBIT_MQ_TAG)
            .WithRabbitEndpoint(amqpPort, httpPort)
            .WithEnvironment(ctx =>
            {
                ctx.EnvironmentVariables[RabbitMQDefaults.DEFAULT_USER_ENV_VAR] = resource.Username;
                ctx.EnvironmentVariables[RabbitMQDefaults.DEFAULT_PASS_ENV_VAR] = resource.Password;
            });

        RegisterDefinitionsLoader(builder, resource, rb);

        return rb;
    }

    /// <summary>
    /// Replaces the Docker image used by the RabbitMQ resource.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> WithDockerImage(
        this IResourceBuilder<RabbitMQResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Configures RabbitMQ access credentials.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> WithCredentials(
        this IResourceBuilder<RabbitMQResource> builder,
        string username,
        string password)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.Username = username;
        builder.Resource.Password = password;
        return builder;
    }

    /// <summary>
    /// Configures exchanges to be automatically created in RabbitMQ via definitions.json.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> WithExchanges(
        this IResourceBuilder<RabbitMQResource> builder,
        params IReadOnlyList<ExchangeConfig> exchanges)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(exchanges);

        builder.Resource.Exchanges ??= [];
        builder.Resource.Exchanges.AddRange(exchanges);
        return builder;
    }

    /// <summary>
    /// Configures queues to be automatically created in RabbitMQ via definitions.json.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> WithQueues(
        this IResourceBuilder<RabbitMQResource> builder,
        params IReadOnlyList<QueueConfig> queues)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(queues);

        builder.Resource.Queues ??= [];
        builder.Resource.Queues.AddRange(queues);
        return builder;
    }

    /// <summary>
    /// Configures a Docker volume for RabbitMQ data persistence.
    /// </summary>
    public static IResourceBuilder<RabbitMQResource> WithDataVolume(
        this IResourceBuilder<RabbitMQResource> builder,
        string volumeName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(volumeName);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(volumeName, RabbitMQDefaults.DATA_VOLUME_PATH);
    }

    /// <summary>
    /// Adds a reference to the RabbitMQ resource in the project, configuring WaitFor
    /// and the connection string via native WithReference.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<RabbitMQResource> rabbitMQ)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(rabbitMQ);

        return project.WithReference(source: rabbitMQ);
    }

    /// <summary>
    /// Configures the AMQP and Management HTTP endpoints for the RabbitMQ resource, without proxy, with health check.
    /// </summary>
    private static IResourceBuilder<RabbitMQResource> WithRabbitEndpoint(
        this IResourceBuilder<RabbitMQResource> resource,
        int amqpPort,
        int httpPort)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amqpPort, IPEndPoint.MinPort);
        ArgumentOutOfRangeException.ThrowIfLessThan(httpPort, IPEndPoint.MinPort);

        return resource.WithEndpoint(
                port: amqpPort,
                targetPort: RabbitMQDefaults.DEFAULT_AMQP_PORT,
                name: RabbitMQResource.AMQP_ENDPOINT_NAME,
                isProxied: false)
            .WithHttpEndpoint(
                port: httpPort,
                targetPort: RabbitMQDefaults.DEFAULT_MANAGEMENT_PORT,
                name: RabbitMQResource.MANAGEMENT_ENDPOINT_NAME,
                isProxied: false)
            .WithHttpHealthCheck(endpointName: RabbitMQResource.MANAGEMENT_ENDPOINT_NAME);
    }

    /// <summary>
    /// Registers the BeforeStartEvent subscriber responsible for generating and mounting the RabbitMQ definitions.json
    /// file before resource initialization, if exchanges or queues are configured.
    /// </summary>
    private static void RegisterDefinitionsLoader(
        IDistributedApplicationBuilder appBuilder,
        RabbitMQResource resource,
        IResourceBuilder<RabbitMQResource> rb)
    {
        // Deferred definitions.json generation — runs after all With* calls
        appBuilder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
        {
            if (resource.Exchanges?.Count > 0 || resource.Queues?.Count > 0)
            {
                var definitions = RabbitMQDefinitionsBuilder.Build(resource);
                var definitionsJson = JsonSerializer.Serialize(definitions, RabbitMQDefinitionsBuilder.JsonOptions);

                rb.WithContainerFiles(RabbitMQDefaults.DEFINITIONS_DIR_PATH,
                [
                    new ContainerFile
                    {
                        Name = RabbitMQDefaults.DEFINITIONS_FILENAME,
                        Contents = definitionsJson,
                        Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                               UnixFileMode.GroupRead | UnixFileMode.OtherRead
                    }
                ])
                .WithEnvironment(RabbitMQDefaults.ADDITIONAL_ERL_ARGS_ENV_VAR, RabbitMQDefaults.ADDITIONAL_ERL_ARGS_VALUE);
            }
            return Task.CompletedTask;
        });
    }
}
