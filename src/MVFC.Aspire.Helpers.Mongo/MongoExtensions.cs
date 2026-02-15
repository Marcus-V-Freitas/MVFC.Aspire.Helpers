namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço MongoDB (com Replica Set)
/// em aplicações distribuídas, utilizando containers baseados na imagem oficial do MongoDB.
/// </summary>
public static class MongoExtensions {
    private static readonly IMongoClientFactory _clientFactory = new MongoClientFactory();

    /// <summary>
    /// Adiciona um recurso de MongoDB configurado como Replica Set à aplicação distribuída.
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        string image = MongoDefaults.DefaultMongoImage,
        string tag = MongoDefaults.DefaultImageTag,
        string? volumeName = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var dockerImage = DockerImageHelper.Build(image, tag);

        return builder.AddContainer(name, dockerImage)
                      .WithReplicaSetArgs()
                      .WithVolumeIfSpecified(volumeName)
                      .WithMongoEndpoint()
                      .WithReplicaSetInitScript();
    }

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do recurso de MongoDB.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WaitForMongoReplicaSet(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<ContainerResource> mongoDbResource,
        string connectionStringSection = MongoDefaults.DefaultConnectionStringSection,
        IReadOnlyCollection<IMongoClassDump>? dumps = null) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(mongoDbResource);

        project.WaitFor(mongoDbResource)
               .WithEnvironment(connectionStringSection, MongoDefaults.DefaultConnectionString);

        if (dumps?.Count > 0) {
            project.OnResourceReady(async (_, _, ct) =>
                await _clientFactory.ExecuteDumpsAsync(MongoDefaults.DefaultConnectionString, dumps, ct));
        }

        return project;
    }

    /// <summary>
    /// Adiciona e integra o recurso de MongoDB (Replica Set) ao projeto.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithMongoReplicaSet(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        string image = MongoDefaults.DefaultMongoImage,
        string tag = MongoDefaults.DefaultImageTag,
        string? volumeName = null,
        string connectionStringSection = MongoDefaults.DefaultConnectionStringSection,
        IReadOnlyCollection<IMongoClassDump>? dumps = null) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(builder);

        var mongo = builder.GetOrAddMongoReplicaSet(name, image, tag, volumeName);
        return project.WaitForMongoReplicaSet(mongo, connectionStringSection, dumps);
    }

    private static IResourceBuilder<ContainerResource> GetOrAddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        string image,
        string tag,
        string? volumeName) {
        if (builder.TryCreateResourceBuilder(name, out IResourceBuilder<ContainerResource>? mongo)) {
            return mongo!;
        }

        return builder.AddMongoReplicaSet(name, image, tag, volumeName);
    }

    private static IResourceBuilder<ContainerResource> WithReplicaSetArgs(
        this IResourceBuilder<ContainerResource> resource) =>
        resource.WithArgs("--replSet", MongoDefaults.ReplicaSetName, "--bind_ip_all");

    private static IResourceBuilder<ContainerResource> WithVolumeIfSpecified(
        this IResourceBuilder<ContainerResource> resource,
        string? volumeName) {
        if (!string.IsNullOrWhiteSpace(volumeName)) {
            resource.WithVolume(volumeName, "/data/db");
        }

        return resource;
    }

    private static IResourceBuilder<ContainerResource> WithMongoEndpoint(
        this IResourceBuilder<ContainerResource> resource) =>
        resource.WithEndpoint(
            MongoDefaults.HostPort,
            MongoDefaults.HostPort,
            "mongodb",
            isProxied: false,
            isExternal: true);

    private static IResourceBuilder<ContainerResource> WithReplicaSetInitScript(
        this IResourceBuilder<ContainerResource> resource) {
        var initScript = ReplicaSetScriptProvider.GetInitScript();

        return resource.WithContainerFiles("/docker-entrypoint-initdb.d",
        [
            new ContainerFile
            {
                Name = "init-replica-set.js",
                Contents = initScript,
                Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                       UnixFileMode.GroupRead | UnixFileMode.OtherRead
            }
        ]);
    }
}
