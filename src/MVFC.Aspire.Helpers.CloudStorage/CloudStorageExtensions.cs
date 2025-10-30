namespace MVFC.Aspire.Helpers.CloudStorage;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço de Cloud Storage (emulador GCS)
/// </summary>
public static class CloudStorageExtensions {
    private const int HOST_PORT = 4443;
    private const char DOCKER_IMAGE_DELIMITER = ':';
    private const string DEFAULT_CLOUD_STORAGE_IMAGE = "fsouza/fake-gcs-server";

    /// <summary>
    /// Adiciona um recurso de Cloud Storage (emulador GCS) à aplicação distribuída, utilizando um container baseado na imagem "fsouza/fake-gcs-server".
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso de Cloud Storage a ser criado.</param>
    /// <param name="image">Nome da imagem Docker utilizada para o emulador CloudStorage.</param>
    /// <param name="tag">Tag da imagem Docker do emulador CloudStorage.</param>
    /// <param name="localBucketFolder">
    /// (Opcional) Caminho local para uma pasta que será montada no container como armazenamento persistente dos buckets.
    /// Se não informado, o armazenamento será volátil.
    /// </param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{ContainerResource}"/> representando o recurso de Cloud Storage configurado.
    /// </returns>
    public static IResourceBuilder<ContainerResource> AddCloudStorage(this IDistributedApplicationBuilder builder, string name, string image = DEFAULT_CLOUD_STORAGE_IMAGE, string tag = "latest", string? localBucketFolder = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        var dockerImage = BuildDockerImage(image, tag);
        var cloudStorage = builder.AddContainer(name, dockerImage)
                                  .WithHttpEndpoint(port: HOST_PORT, targetPort: HOST_PORT, name: "http", isProxied: false)
                                  .WithArgs("--scheme", "http");

        if (!string.IsNullOrWhiteSpace(localBucketFolder))
            cloudStorage.WithBindMount(localBucketFolder, "/data");

        return cloudStorage;
    }

    /// <summary>
    /// Constrói o nome completo da imagem Docker utilizando o nome da imagem e a tag informada.
    /// </summary>
    /// <param name="image">Nome da imagem Docker.</param>
    /// <param name="tag">Tag da imagem Docker.</param>
    /// <returns>String no formato "image:tag" para uso em containers.</returns>
    private static string BuildDockerImage(string image, string tag) =>
        new StringBuilder(image)
                .Append(DOCKER_IMAGE_DELIMITER)
                .Append(tag)
                .ToString();

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do recurso de Cloud Storage e define a variável de ambiente necessária para o emulador.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá depender do Cloud Storage.</param>
    /// <param name="cloudStorageResource">O recurso de Cloud Storage a ser aguardado.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para aguardar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WaitForCloudStorage(this IResourceBuilder<ProjectResource> project, IResourceBuilder<ContainerResource> cloudStorageResource) {
        project.WaitFor(cloudStorageResource)
               .WithEnvironment("STORAGE_EMULATOR_HOST", BuildLocalEndpoint(HOST_PORT));

        return project;
    }

    /// <summary>
    /// Constrói a URL local do endpoint do emulador de Cloud Storage, utilizando o número da porta informado.
    /// </summary>
    /// <param name="port">Porta TCP na qual o emulador está exposto localmente.</param>
    /// <returns>
    /// String representando o endpoint local do serviço de Cloud Storage, no formato "http://localhost:{port}/storage/v1/".
    /// </returns>
    private static string BuildLocalEndpoint(int port) =>
        new StringBuilder("http://localhost:")
                .Append(port)
                .Append("/storage/v1/")
                .ToString();

    /// <summary>
    /// Adiciona e integra o recurso de Cloud Storage ao projeto, configurando dependências e persistência opcional.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o Cloud Storage.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de Cloud Storage a ser criado.</param>
    /// <param name="image">Nome da imagem Docker utilizada para o emulador CloudStorage.</param>
    /// <param name="tag">Tag da imagem Docker do emulador Pub/Sub.</param>
    /// <param name="localBucketFolder">
    /// (Opcional) Caminho local para uma pasta que será montada no container como armazenamento persistente dos buckets.
    /// </param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithCloudStorage(this IResourceBuilder<ProjectResource> project, IDistributedApplicationBuilder builder, string name, string image = DEFAULT_CLOUD_STORAGE_IMAGE, string tag = "latest", string? localBucketFolder = null) {
        var cloudStorage = builder.AddCloudStorage(name, image, tag, localBucketFolder);

        return project.WaitForCloudStorage(cloudStorage);
    }
}