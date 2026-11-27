namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of a MongoDB service (with Replica Set)
/// in distributed applications using containers based on the official MongoDB image.
/// </summary>
public static class MongoExtensions
{
    /// <summary>
    /// Adds an opt-in writable-primary probe with a three-second deadline per check.
    /// Consumers must use WaitFor explicitly. Dump completion is not part of this check.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> WithPrimaryHealthCheck(
        this IResourceBuilder<MongoReplicaSetResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var key = $"mongo_{builder.Resource.Name}_primary";
        if (builder.Resource.Annotations.OfType<HealthCheckAnnotation>().Any(annotation => annotation.Key == key))
            return builder;

        var check = new MongoPrimaryHealthCheck(builder.Resource.MongoEndpoint);
        builder.ApplicationBuilder.Services.AddHealthChecks().Add(new HealthCheckRegistration(
            key, _ => check, failureStatus: null, tags: null, timeout: TimeSpan.FromSeconds(3)));

        return builder.WithHealthCheck(key);
    }

    /// <summary>
    /// Adds a MongoDB resource configured as a Replica Set to the distributed application.
    /// Use fluent methods such as WithDumps and WithDataVolume to customize.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> AddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = MongoDefaults.HOST_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new MongoReplicaSetResource(name);

        return builder.AddResource(resource)
                      .WithDockerImage(
                           image: MongoDefaults.DEFAULT_MONGO_IMAGE,
                           tag: MongoDefaults.DEFAULT_IMAGE_TAG)
                      .WithImageSHA256(MongoDefaults.DEFAULT_IMAGE_SHA256)
                      .WithReplicaSetArgs()
                      .WithMongoEndpoint(port)
                      .WithReplicaSetInitScript();
    }

    /// <summary>
    /// Replaces the Docker image used by the MongoDB resource.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> WithDockerImage(
        this IResourceBuilder<MongoReplicaSetResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(image);
        ArgumentNullException.ThrowIfNullOrEmpty(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Configures data dumps to be executed when the first referencing project becomes ready.
    /// Can be called before or after WithReference.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> WithDumps(
        this IResourceBuilder<MongoReplicaSetResource> builder,
        params IReadOnlyCollection<IMongoClassDump> dumps)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(dumps);

        builder.Resource.Dumps = dumps;
        return builder;
    }

    /// <summary>
    /// Configures a Docker volume for MongoDB data persistence.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> WithDataVolume(
        this IResourceBuilder<MongoReplicaSetResource> builder,
        string volumeName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(volumeName);

        return builder.WithVolume(volumeName, MongoDefaults.DATA_VOLUME_PATH);
    }

    /// <summary>
    /// Adds a reference to the MongoDB resource in the project, configuring an environment variable
    /// with the connection string, and automatic dump execution (if configured).
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MongoReplicaSetResource> mongo)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(mongo);

        project.WithReference(source: mongo)
               .WithEnvironment(MongoDefaults.DEFAULT_CONNECTION_STRING_SECTION, mongo.Resource.ConnectionStringExpression);

        RegisterDumpsExecutor(project, mongo);

        return project;
    }

    /// <summary>
    /// Waits for a writable primary and successful dump completion before starting this consumer.
    /// Adds a startup dependency; use WithReference separately to inject the connection string.
    /// Shared dumps run once per AppHost lifetime, even with multiple consumers.
    /// </summary>
    /// <param name="project">The consumer whose startup requires the configured data.</param>
    /// <param name="mongo">The MongoDB resource containing the dumps.</param>
    /// <returns>The consumer builder for chaining.</returns>
    public static IResourceBuilder<ProjectResource> WaitForDumps(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MongoReplicaSetResource> mongo)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(mongo);

        if (project.Resource.Annotations.OfType<MongoDumpsWaitAnnotation>().Any(annotation => ReferenceEquals(annotation.Resource, mongo.Resource)))
            return project;

        project.WithAnnotation(new MongoDumpsWaitAnnotation(mongo.Resource));
        project.WaitFor(mongo.WithPrimaryHealthCheck());
        var execution = RegisterDumpsExecutor(project, mongo);

        return project.OnBeforeResourceStarted(async (_, startEvent, ct) =>
        {
            var notifications = startEvent.Services.GetRequiredService<ResourceNotificationService>();
            var lifetime = startEvent.Services.GetRequiredService<IHostApplicationLifetime>();
            await execution.WaitAsync(mongo.Resource,
                notifications.WaitForResourceHealthyAsync(mongo.Resource.Name, ct),
                lifetime.ApplicationStopping, ct).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Registers the OnResourceReady callback responsible for executing MongoDB dumps after
    /// resource initialization, ensuring single execution via annotation.
    /// </summary>
    private static MongoDumpsExecutedAnnotation RegisterDumpsExecutor(
        IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MongoReplicaSetResource> mongo)
    {
        var existing = mongo.Resource.Annotations.OfType<MongoDumpsExecutedAnnotation>().SingleOrDefault();
        if (existing is not null)
            return existing;

        var execution = new MongoDumpsExecutedAnnotation();
        mongo.WithAnnotation(execution);

        project.OnResourceReady((_, _, ct) => execution.ExecuteAsync(mongo.Resource, ct));

        return execution;
    }

    /// <summary>
    /// Adds the arguments required to enable the Replica Set.
    /// </summary>
    private static IResourceBuilder<MongoReplicaSetResource> WithReplicaSetArgs(
        this IResourceBuilder<MongoReplicaSetResource> resource) =>
            resource.WithArgs(MongoDefaults.REPL_SET_ARG, MongoDefaults.REPLICA_SET_NAME, MongoDefaults.BIND_IP_ALL_ARG);

    /// <summary>
    /// Configures the MongoDB endpoint in the container.
    /// </summary>
    private static IResourceBuilder<MongoReplicaSetResource> WithMongoEndpoint(
        this IResourceBuilder<MongoReplicaSetResource> resource, int port)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource.WithEndpoint(
            port: port,
            targetPort: MongoDefaults.HOST_PORT,
            scheme: MongoReplicaSetResource.MONGO_ENDPOINT_NAME,
            name: MongoReplicaSetResource.MONGO_ENDPOINT_NAME,
            isProxied: true,
            isExternal: true);
    }

    /// <summary>
    /// Adds the Replica Set initialization script to the container.
    /// </summary>
    private static IResourceBuilder<MongoReplicaSetResource> WithReplicaSetInitScript(
        this IResourceBuilder<MongoReplicaSetResource> resource)
    {
        var initScript = ReplicaSetScriptProvider.GetInitScript();

        return resource.WithContainerFiles(MongoDefaults.INIT_SCRIPTS_PATH,
        [
            new ContainerFile
            {
                Name = MongoDefaults.REPLICA_SET_INIT_SCRIPT_FILENAME,
                Contents = initScript,
                Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                       UnixFileMode.GroupRead | UnixFileMode.OtherRead
            }
        ]);
    }
}
