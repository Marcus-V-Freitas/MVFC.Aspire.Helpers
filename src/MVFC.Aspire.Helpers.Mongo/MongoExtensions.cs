namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of a MongoDB service (with Replica Set)
/// in distributed applications using containers based on the official MongoDB image.
/// </summary>
public static class MongoExtensions
{
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
    /// Configures data dumps to be executed when the resource is referenced by projects.
    /// </summary>
    public static IResourceBuilder<MongoReplicaSetResource> WithDumps(
        this IResourceBuilder<MongoReplicaSetResource> builder,
        IReadOnlyCollection<IMongoClassDump> dumps)
    {
        ArgumentNullException.ThrowIfNull(builder);

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
    /// Adds a reference to the MongoDB resource in the project, configuring WaitFor, environment variable
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
    /// Registers the OnResourceReady callback responsible for executing MongoDB dumps after
    /// resource initialization, ensuring single execution via annotation.
    /// </summary>
    private static void RegisterDumpsExecutor(
        IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MongoReplicaSetResource> mongo)
    {
        var dumps = mongo.Resource.Dumps;
        if (dumps?.Count is not > 0)
            return;

        if (mongo.Resource.TryGetAnnotationsOfType<MongoDumpsExecutedAnnotation>(out _))
            return;

        mongo.WithAnnotation(new MongoDumpsExecutedAnnotation());

        project.OnResourceReady(async (_, _, ct) =>
        {
            var connectionString = await mongo.Resource.ConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false);
            await MongoClientFactory.ExecuteDumpsAsync(connectionString!, dumps, ct).ConfigureAwait(false);
        });
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
            isProxied: false,
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
