namespace MVFC.Aspire.Helpers.CloudStorage;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço de Cloud Storage (emulador GCS)
/// em aplicações distribuídas, utilizando containers baseados na imagem "fsouza/fake-gcs-server".
/// </summary>
public static class CloudStorageExtensions
{
    private const string DEFAULT_CLOUD_STORAGE_IMAGE = "fsouza/fake-gcs-server";

    /// <summary>
    /// Adiciona um recurso de Cloud Storage (emulador GCS) à aplicação distribuída, utilizando um container baseado na imagem "fsouza/fake-gcs-server".
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso de Cloud Storage a ser criado.</param>
    /// <param name="localBucketFolder">
    /// (Opcional) Caminho local para uma pasta que será montada no container como armazenamento persistente dos buckets.
    /// Se não informado, o armazenamento será volátil.
    /// </param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{ContainerResource}"/> representando o recurso de Cloud Storage configurado.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada se o parâmetro <paramref name="name"/> for nulo, vazio ou composto apenas por espaços em branco.
    /// </exception>
    public static IResourceBuilder<ContainerResource> AddCloudStorage(this IDistributedApplicationBuilder builder, string name, string? localBucketFolder = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var cloudStorage = builder.AddContainer(name, DEFAULT_CLOUD_STORAGE_IMAGE)
                                  .WithHttpEndpoint(port: 4443, targetPort: 4443, name: "http", isProxied: false)
                                  .WithArgs("--scheme", "http");

        if (!string.IsNullOrWhiteSpace(localBucketFolder))
            cloudStorage.WithBindMount(localBucketFolder, "/data");

        return cloudStorage;
    }

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do recurso de Cloud Storage e define a variável de ambiente necessária para o emulador.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá depender do Cloud Storage.</param>
    /// <param name="cloudStorageResource">O recurso de Cloud Storage a ser aguardado.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para aguardar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WaitForCloudStorage(this IResourceBuilder<ProjectResource> project, IResourceBuilder<ContainerResource> cloudStorageResource)
    {
        project.WaitFor(cloudStorageResource)
               .WithEnvironment("STORAGE_EMULATOR_HOST", "localhost:4443/storage/v1/");

        return project;
    }

    /// <summary>
    /// Adiciona e integra o recurso de Cloud Storage ao projeto, configurando dependências e persistência opcional.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o Cloud Storage.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de Cloud Storage a ser criado.</param>
    /// <param name="localBucketFolder">
    /// (Opcional) Caminho local para uma pasta que será montada no container como armazenamento persistente dos buckets.
    /// </param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Cloud Storage.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithCloudStorage(this IResourceBuilder<ProjectResource> project, IDistributedApplicationBuilder builder, string name, string? localBucketFolder = null)
    {
        var cloudStorage = builder.AddCloudStorage(name, localBucketFolder);

        return project.WaitForCloudStorage(cloudStorage);
    }
}