namespace MVFC.Aspire.Helpers.Gotenberg;

/// <summary>
/// Métodos de extensão para simplificar a configuração e integração do recurso Gotenberg
/// em aplicações distribuídas usando Aspire.
/// </summary>
public static class GotenbergExtensions
{
    /// <summary>
    /// Adiciona um recurso Gotenberg à aplicação distribuída com configurações padrão.
    /// Use métodos fluentes como <see cref="WithDockerImage"/> para customizar.
    /// </summary>
    /// <param name="builder">Builder da aplicação distribuída.</param>
    /// <param name="name">Nome lógico do recurso Gotenberg.</param>
    /// <param name="port">
    /// Porta HTTP exposta no host. Internamente o Gotenberg continua ouvindo na porta 3000.
    /// </param>
    /// <returns>Builder do recurso Gotenberg.</returns>
    public static IResourceBuilder<GotenbergResource> AddGotenberg(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = GotenbergDefaults.DEFAULT_HTTP_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        var resource = new GotenbergResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: GotenbergDefaults.DEFAULT_IMAGE,
                tag: GotenbergDefaults.DEFAULT_IMAGE_TAG)
            .WithGotenbergEndpoint(port);
    }

    /// <summary>
    /// Substitui a imagem Docker usada pelo recurso Gotenberg.
    /// </summary>
    /// <param name="builder">Builder do recurso Gotenberg.</param>
    /// <param name="image">Nome da imagem Docker.</param>
    /// <param name="tag">Tag da imagem Docker.</param>
    /// <returns>O próprio builder, para encadeamento fluente.</returns>
    public static IResourceBuilder<GotenbergResource> WithDockerImage(
        this IResourceBuilder<GotenbergResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(image);
        ArgumentNullException.ThrowIfNullOrEmpty(tag);

        return builder.WithImage(image)
                      .WithImageTag(tag);
    }

    /// <summary>
    /// Adiciona uma referência ao recurso Gotenberg em um projeto Aspire,
    /// configurando também uma variável de ambiente com a base URL do serviço.
    /// </summary>
    /// <param name="project">Builder do recurso de projeto.</param>
    /// <param name="gotenberg">Builder do recurso Gotenberg.</param>
    /// <returns>O próprio builder de projeto, para encadeamento fluente.</returns>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<GotenbergResource> gotenberg)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(gotenberg);

        return project
            .WithReference(source: gotenberg)
            .WithEnvironment(
                GotenbergDefaults.BASE_URL_ENV_VAR,
                gotenberg.Resource.ConnectionStringExpression);
    }

    /// <summary>
    /// Configura o endpoint HTTP do Gotenberg (sem proxy) e registra o health check em /health.
    /// </summary>
    /// <param name="resource">Builder do recurso Gotenberg.</param>
    /// <param name="port">Porta HTTP exposta no host.</param>
    /// <returns>O próprio builder do recurso.</returns>
    private static IResourceBuilder<GotenbergResource> WithGotenbergEndpoint(
        this IResourceBuilder<GotenbergResource> resource,
        int port)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        return resource
            .WithHttpEndpoint(
                port: port,
                targetPort: GotenbergDefaults.DEFAULT_HTTP_PORT,
                name: GotenbergResource.HTTP_ENDPOINT_NAME,
                isProxied: false)
            .WithHttpHealthCheck(GotenbergDefaults.HEALTH_PATH);
    }
}
