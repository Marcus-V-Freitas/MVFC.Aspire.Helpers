namespace MVFC.Aspire.Helpers.CloudStorage;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração, integração e uso de um recurso de Cloud Storage (emulador GCS) em aplicações distribuídas,
/// permitindo customização de imagem, persistência de buckets e integração com projetos dependentes.
/// </summary>
public static class CloudStorageExtensions {
    private const int HostPort = 4443;
    private const string StoragePathSuffix = "/storage/v1/";

    /// <summary>
    /// Adiciona um recurso de Cloud Storage (emulador GCS) à aplicação distribuída, utilizando um container baseado na imagem "fsouza/fake-gcs-server".
    /// Permite customizar configurações como tag da imagem, host público e persistência dos buckets.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso de Cloud Storage a ser criado.</param>
    /// <param name="storageConfig">Configurações opcionais para o recurso de Cloud Storage.</param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{CloudStorageResource}"/> representando o recurso de Cloud Storage configurado.
    /// </returns>
    public static IResourceBuilder<CloudStorageResource> AddCloudStorage(
        this IDistributedApplicationBuilder builder,
        string name,
        CloudStorageConfig? storageConfig = null) {

        storageConfig ??= new CloudStorageConfig();

        var resource = new CloudStorageResource(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(storageConfig.EmulatorImage)
            .WithImageTag(storageConfig.EmulatorTag)
            .WithHttpEndpoint(
                port: HostPort,
                targetPort: HostPort,
                name: CloudStorageResource.HttpEndpointName,
                isProxied: false)
            .WithArgs("--scheme", CloudStorageResource.HttpEndpointName);

        if (!string.IsNullOrWhiteSpace(storageConfig.LocalBucketFolder)) {
            resourceBuilder.WithBindMount(storageConfig.LocalBucketFolder, "/data");
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Sobrecarga simplificada com porta customizada
    /// </summary>
    public static IResourceBuilder<CloudStorageResource> AddCloudStorage(
        this IDistributedApplicationBuilder builder,
        string name,
        string? localBucketFolder = null) =>

        builder.AddCloudStorage(name, new CloudStorageConfig(
            LocalBucketFolder: localBucketFolder
        ));

    /// <summary>
    /// Adiciona e integra o recurso de Cloud Storage ao projeto, configurando dependências e persistência opcional.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o Cloud Storage.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de Cloud Storage.</param>
    /// <param name="storageConfig">Configurações opcionais para o recurso de Cloud Storage.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithCloudStorage(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        CloudStorageConfig? storageConfig = null) {

        var cloudStorage = builder.AddCloudStorage(name, storageConfig);

        return project
            .WithReference(cloudStorage)
            .WithEnvironment("STORAGE_EMULATOR_HOST", GetStorageEndpointUrl(cloudStorage.Resource))
            .WaitFor(cloudStorage);
    }

    /// <summary>
    /// Adiciona e integra o recurso de Cloud Storage ao projeto, permitindo informar apenas o nome e o caminho local para persistência dos buckets.
    /// </summary>
    /// <param name="project">O recurso do projeto.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de Cloud Storage.</param>
    /// <param name="localBucketFolder">Caminho local para persistência dos buckets (opcional).</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithCloudStorage(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        string? localBucketFolder = null) {

        var settings = new CloudStorageConfig(
            LocalBucketFolder: localBucketFolder
        );

        return WithCloudStorage(project, builder, name, settings);
    }

    /// <summary>
    /// Adiciona e integra o recurso de Cloud Storage ao projeto, permitindo customização das configurações via callback.
    /// </summary>
    /// <param name="project">O recurso do projeto.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="configure">Callback para customizar as configurações do Cloud Storage.</param>
    /// <param name="name">Nome do recurso de Cloud Storage.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithCloudStorage(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        Func<CloudStorageConfig, CloudStorageConfig> configure,
        string name) {

        IResourceBuilder<CloudStorageResource> cloudStorage;

        if (!builder.TryCreateResourceBuilder(name, out cloudStorage!)) {
            var settings = configure(new CloudStorageConfig());
            cloudStorage = builder.AddCloudStorage(name, settings);
        }

        return project
            .WithReference(cloudStorage)
            .WithEnvironment("STORAGE_EMULATOR_HOST", GetStorageEndpointUrl(cloudStorage.Resource))
            .WaitFor(cloudStorage);
    }

    /// <summary>
    /// Constrói a URL completa do endpoint do emulador de Cloud Storage, incluindo o sufixo "/storage/v1/".
    /// </summary>
    /// <param name="resource">O recurso de Cloud Storage.</param>
    /// <returns>
    /// Expressão de referência contendo a URL do endpoint.
    /// </returns>
    private static ReferenceExpression GetStorageEndpointUrl(CloudStorageResource resource) {
        var endpoint = resource.HttpEndpoint;
        return ReferenceExpression.Create(
            $"{endpoint.Property(EndpointProperty.Scheme)}://{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}{StoragePathSuffix}"
        );
    }
}