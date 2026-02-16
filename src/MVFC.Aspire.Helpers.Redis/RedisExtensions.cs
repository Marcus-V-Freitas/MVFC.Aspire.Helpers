namespace MVFC.Aspire.Helpers.Redis;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração do recurso Redis
/// em aplicações distribuídas utilizando o Aspire.
/// </summary>
public static class RedisExtensions
{
    /// <summary>
    /// Adiciona um recurso Redis à aplicação distribuída.
    /// Permite configurar opções como imagem, porta, senha, Redis Commander e persistência de dados.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso Redis.</param>
    /// <param name="redisConfig">Configurações opcionais para o Redis. Se nulo, utiliza valores padrão.</param>
    /// <returns>Um <see cref="IResourceBuilder{RedisResource}"/> configurado.</returns>
    public static IResourceBuilder<RedisResource> AddRedis(
        this IDistributedApplicationBuilder builder,
        string name,
        RedisConfig? redisConfig = null)
    {

        redisConfig ??= new RedisConfig();

        var resource = new RedisResource(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(redisConfig.ImageName)
            .WithImageTag(redisConfig.ImageTag)
            .WithEndpoint(
                port: redisConfig.Port,
                targetPort: RedisDefaults.DefaultRedisPort,
                name: RedisResource.RedisEndpointName);

        // Configurar senha se fornecida
        if (!string.IsNullOrWhiteSpace(redisConfig.Password))
        {
            resourceBuilder.WithEnvironment("REDIS_PASSWORD", redisConfig.Password);
            resourceBuilder.WithArgs("--requirepass", redisConfig.Password);
        }

        // Configurar persistência se volume especificado
        if (!string.IsNullOrWhiteSpace(redisConfig.VolumeName))
        {
            resourceBuilder
                .WithVolume(redisConfig.VolumeName, "/data")
                .WithArgs("--appendonly", "yes");
        }

        // Adicionar Redis Commander se solicitado
        if (redisConfig.WithCommander)
        {
            var commanderName = $"{name}-commander";
            builder.AddContainer(commanderName, redisConfig.CommanderImageName, redisConfig.CommanderImageTag)
                .WithHttpEndpoint(
                    port: redisConfig.CommanderPort,
                    targetPort: RedisDefaults.DefaultCommanderPort,
                    name: "http")
                .WithEnvironment("REDIS_HOST", name)
                .WithEnvironment("REDIS_PORT", RedisDefaults.DefaultRedisPort.ToString())
                .WithReference(resourceBuilder);
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Aguarda até que o recurso Redis esteja disponível antes de iniciar o projeto Aspire.
    /// Adiciona uma referência ao recurso Redis, garantindo que o projeto só será iniciado após o Redis estar pronto.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto Aspire.</param>
    /// <param name="redis">O builder do recurso Redis.</param>
    /// <returns>O builder do recurso do projeto Aspire com dependência do Redis.</returns>
    public static IResourceBuilder<ProjectResource> WaitForRedis(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<RedisResource> redis) =>

        project.WaitFor(redis)
               .WithReference(redis);

    /// <summary>
    /// Adiciona uma referência ao recurso Redis em um projeto Aspire.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso Redis.</param>
    /// <param name="redisConfig">Configurações opcionais para o Redis.</param>
    /// <param name="connectionStringSection">Seção da connection string na configuração da aplicação.</param>
    /// <returns>O builder do recurso do projeto com a referência ao Redis.</returns>
    public static IResourceBuilder<ProjectResource> WithRedis(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        RedisConfig? redisConfig = null,
        string connectionStringSection = RedisDefaults.DefaultConnectionStringSection)
    {

        IResourceBuilder<RedisResource> redis;

        if (!builder.TryCreateResourceBuilder(name, out redis!))
        {
            redis = builder.AddRedis(name, redisConfig);
        }

        return project.WaitForRedis(redis)
                      .WithEnvironment(connectionStringSection, redis.Resource.ConnectionStringExpression);
    }
}
