namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço MongoDB (com Replica Set)
/// em aplicações distribuídas, utilizando containers baseados na imagem oficial do MongoDB.
/// </summary>
public static class MongoExtensions
{
    /// <summary>
    /// Adiciona um recurso de MongoDB configurado como Replica Set à aplicação distribuída.
    /// </summary>
    /// <param name="port">Porta do host. Se null, o Aspire aloca uma porta dinâmica.</param>
    public static IResourceBuilder<ContainerResource> AddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string image = MongoDefaults.DefaultMongoImage,
        string tag = MongoDefaults.DefaultImageTag,
        string? volumeName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var dockerImage = DockerImageHelper.Build(image, tag);

        return builder.AddContainer(name, dockerImage)
                      .WithReplicaSetArgs()
                      .WithVolumeIfSpecified(volumeName)
                      .WithMongoEndpoint(port)
                      .WithReplicaSetInitScript();
    }

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do recurso de MongoDB.
    /// A connection string utiliza <c>directConnection=true</c> para evitar que o driver
    /// redirecione para o hostname interno do Replica Set (<c>localhost:27017</c> dentro do container),
    /// o que causaria falha de conexão a partir do host.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WaitForMongoReplicaSet(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<ContainerResource> mongoDbResource,
        string connectionStringSection = MongoDefaults.DefaultConnectionStringSection,
        IReadOnlyCollection<IMongoClassDump>? dumps = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(mongoDbResource);

        project.WaitFor(mongoDbResource)
               .WithEnvironment(ctx =>
               {
                   var endpoint = mongoDbResource.Resource.GetEndpoint(MongoDefaults.EndpointName);
                   ctx.EnvironmentVariables[connectionStringSection] = BuildConnectionString(endpoint.Port);
               });

        if (dumps?.Count > 0)
        {
            project.OnResourceReady(async (_, _, ct) =>
            {
                var endpoint = mongoDbResource.Resource.GetEndpoint(MongoDefaults.EndpointName);
                await MongoClientFactory.ExecuteDumpsAsync(BuildConnectionString(endpoint.Port), dumps, ct);
            });
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
        int? port = null,
        string image = MongoDefaults.DefaultMongoImage,
        string tag = MongoDefaults.DefaultImageTag,
        string? volumeName = null,
        string connectionStringSection = MongoDefaults.DefaultConnectionStringSection,
        IReadOnlyCollection<IMongoClassDump>? dumps = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(builder);

        var mongo = builder.GetOrAddMongoReplicaSet(name, port, image, tag, volumeName);
        return project.WaitForMongoReplicaSet(mongo, connectionStringSection, dumps);
    }

    /// <summary>
    /// Monta a connection string com <c>directConnection=true</c> e <c>replicaSet</c>,
    /// garantindo que o driver não tente resolver o hostname interno do Replica Set.
    /// </summary>
    private static string BuildConnectionString(int port) =>
        $"mongodb://localhost:{port}/?directConnection=true";

    /// <summary>
    /// Obtém um recurso de MongoDB Replica Set existente pelo nome ou adiciona um novo recurso se não existir.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de MongoDB.</param>
    /// <param name="port">Porta do host. Se null, será utilizada a porta padrão do container.</param>
    /// <param name="image">Nome da imagem Docker do MongoDB.</param>
    /// <param name="tag">Tag da imagem Docker.</param>
    /// <param name="volumeName">Nome do volume Docker para persistência de dados (opcional).</param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{ContainerResource}"/> representando o recurso de MongoDB configurado.
    /// </returns>
    private static IResourceBuilder<ContainerResource> GetOrAddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port,
        string image,
        string tag,
        string? volumeName)
    {
        if (builder.TryCreateResourceBuilder(name, out IResourceBuilder<ContainerResource>? mongo))
            return mongo!;

        return builder.AddMongoReplicaSet(name, port, image, tag, volumeName);
    }

    /// <summary>
    /// Adiciona os argumentos necessários para habilitar o Replica Set no container do MongoDB.
    /// </summary>
    /// <param name="resource">O recurso do container MongoDB.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ContainerResource}"/> com os argumentos de Replica Set configurados.
    /// </returns>
    private static IResourceBuilder<ContainerResource> WithReplicaSetArgs(
        this IResourceBuilder<ContainerResource> resource) =>
        resource.WithArgs("--replSet", MongoDefaults.ReplicaSetName, "--bind_ip_all");

    /// <summary>
    /// Adiciona o volume de persistência ao container do MongoDB, se um nome de volume for especificado.
    /// </summary>
    /// <param name="resource">O recurso do container MongoDB.</param>
    /// <param name="volumeName">Nome do volume Docker para persistência de dados (opcional).</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ContainerResource}"/> com o volume configurado, se aplicável.
    /// </returns>
    private static IResourceBuilder<ContainerResource> WithVolumeIfSpecified(
        this IResourceBuilder<ContainerResource> resource,
        string? volumeName)
    {
        if (!string.IsNullOrWhiteSpace(volumeName))
            resource.WithVolume(volumeName, "/data/db");

        return resource;
    }

    /// <summary>
    /// Configura o endpoint do MongoDB no container, expondo a porta especificada ou a porta padrão.
    /// </summary>
    /// <param name="resource">O recurso do container MongoDB.</param>
    /// <param name="port">Porta do host a ser exposta (opcional).</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ContainerResource}"/> com o endpoint configurado.
    /// </returns>
    private static IResourceBuilder<ContainerResource> WithMongoEndpoint(
        this IResourceBuilder<ContainerResource> resource,
        int? port = null) =>
        resource.WithEndpoint(
            port: port ?? MongoDefaults.ContainerPort,
            targetPort: MongoDefaults.ContainerPort,
            scheme: MongoDefaults.EndpointName,
            name: MongoDefaults.EndpointName,
            isProxied: false,
            isExternal: true);

    /// <summary>
    /// Adiciona o script de inicialização do Replica Set ao container do MongoDB.
    /// </summary>
    /// <param name="resource">O recurso do container MongoDB.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ContainerResource}"/> com o script de inicialização configurado.
    /// </returns>
    private static IResourceBuilder<ContainerResource> WithReplicaSetInitScript(
        this IResourceBuilder<ContainerResource> resource)
    {
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
