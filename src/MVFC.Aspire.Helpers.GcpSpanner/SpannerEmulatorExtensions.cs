namespace MVFC.Aspire.Helpers.GcpSpanner;

/// <summary>
/// Extension methods para registrar o Google Cloud Spanner emulator no Aspire.
/// </summary>
public static class SpannerEmulatorExtensions
{
    /// <summary>
    /// Adds the Google Cloud Spanner emulator to the distributed application.
    /// </summary>
    public static IResourceBuilder<SpannerEmulatorResource> AddGcpSpanner(
        this IDistributedApplicationBuilder builder,
        string name,
        int port = SpannerDefaults.GRPC_PORT)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        var resource = new SpannerEmulatorResource(name);

        return builder.AddResource(resource)
                      .WithDockerImage(
                          image: SpannerDefaults.EMULATOR_IMAGE,
                          tag: SpannerDefaults.EMULATOR_IMAGE_TAG)
                      .WithSpannerEndpoint(builder, port, name);
    }

    /// <summary>
    /// Adiciona configurações de instância + database a serem provisionadas no emulador.
    /// </summary>
    public static IResourceBuilder<SpannerEmulatorResource> WithSpannerConfigs(
        this IResourceBuilder<SpannerEmulatorResource> builder,
        params SpannerConfig[] configs)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configs);

        builder.Resource.SpannerConfigs = [.. builder.Resource.SpannerConfigs, .. configs];
        return builder;
    }

    /// <summary>
    /// Substitui a imagem Docker usada pelo recurso Spanner.
    /// </summary>
    /// <param name="builder">Builder do recurso Spanner.</param>
    /// <param name="image">Nome da imagem Docker.</param>
    /// <param name="tag">Tag da imagem Docker.</param>
    /// <returns>O próprio builder, para encadeamento fluente.</returns>
    public static IResourceBuilder<SpannerEmulatorResource> WithDockerImage(
        this IResourceBuilder<SpannerEmulatorResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);

        return builder.WithImage(image)
                      .WithImageTag(tag);
    }

    /// <summary>
    /// Define o timeout máximo de espera pela inicialização do emulador.
    /// </summary>
    public static IResourceBuilder<SpannerEmulatorResource> WithWaitTimeout(
        this IResourceBuilder<SpannerEmulatorResource> builder,
        int seconds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThan(seconds, 0);

        builder.Resource.WaitTimeoutSeconds = seconds;
        return builder;
    }

    /// <summary>
    /// Adiciona referência ao Spanner no projeto consumidor, injetando SPANNER_EMULATOR_HOST
    /// e registrando o provisionamento de instâncias/databases via OnResourceReady.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<SpannerEmulatorResource> spanner)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(spanner);

        project.WithReference(source: spanner)
               .WithEnvironment(SpannerDefaults.EMULATOR_HOST_ENV_VAR, spanner.Resource.ConnectionStringExpression);

        RegisterSpannerConfigurator(spanner);

        return project;
    }

    /// <summary>
    /// Registra um health check TCP para o Spanner Emulator na porta gRPC especificada.
    /// </summary>
    /// <param name="builder">Builder da aplicação distribuída.</param>
    /// <param name="port">Porta gRPC do emulador.</param>
    /// <param name="name">Nome do recurso Spanner.</param>
    /// <returns>Chave de identificação do health check registrado.</returns>
    private static string RegisterTcpHealthCheck(
        this IDistributedApplicationBuilder builder,
        int port,
        string name)
    {
        var healthCheckKey = $"spanner_{name}";

        builder.Services
                      .AddHealthChecks()
                      .Add(new HealthCheckRegistration(
                          name: healthCheckKey,
                          factory: _ => new SpannerGrpcHealthCheck(port),
                          failureStatus: null,
                          tags: null));

        return healthCheckKey;
    }

    /// <summary>
    /// Adiciona o endpoint gRPC e health check ao recurso Spanner Emulator.
    /// </summary>
    /// <param name="resource">Builder do recurso Spanner Emulator.</param>
    /// <param name="builder">Builder da aplicação distribuída.</param>
    /// <param name="port">Porta gRPC a ser exposta.</param>
    /// <param name="name">Nome do recurso Spanner.</param>
    /// <returns>O próprio builder do recurso, para encadeamento fluente.</returns>
    private static IResourceBuilder<SpannerEmulatorResource> WithSpannerEndpoint(
        this IResourceBuilder<SpannerEmulatorResource> resource,
        IDistributedApplicationBuilder builder,
        int port,
        string name)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, IPEndPoint.MinPort);

        var healthCheckKey = builder.RegisterTcpHealthCheck(port, name);

        return resource.WithEndpoint(
                          port: port,
                          targetPort: SpannerDefaults.GRPC_PORT,
                          name: SpannerEmulatorResource.GRPC_ENDPOINT_NAME,
                          isProxied: false)
                      .WithHealthCheck(healthCheckKey);
    }

    /// <summary>
    /// Registra a configuração do Spanner Emulator para provisionamento de instâncias e databases
    /// quando o recurso estiver pronto, evitando duplicidade de configuração.
    /// </summary>
    /// <param name="spanner">Builder do recurso Spanner Emulator.</param>
    private static void RegisterSpannerConfigurator(
        IResourceBuilder<SpannerEmulatorResource> spanner)
    {
        if (spanner.Resource.SpannerConfigs.Count == 0)
            return;

        if (spanner.Resource.TryGetAnnotationsOfType<SpannerConfiguredAnnotation>(out _))
            return;

        spanner.WithAnnotation(new SpannerConfiguredAnnotation());

        spanner.OnResourceReady(async (_, _, ct) =>
        {
            var connectionString = await spanner.Resource.ConnectionStringExpression.GetValueAsync(ct);
            Environment.SetEnvironmentVariable(SpannerDefaults.EMULATOR_HOST_ENV_VAR, connectionString);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(spanner.Resource.WaitTimeoutSeconds));

            try
            {
                await SpannerConfigProcessor.ConfigureAsync(spanner.Resource.SpannerConfigs, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                throw new TimeoutException($"Spanner configuration timed out after {spanner.Resource.WaitTimeoutSeconds} seconds.");
            }
        });
    }
}
