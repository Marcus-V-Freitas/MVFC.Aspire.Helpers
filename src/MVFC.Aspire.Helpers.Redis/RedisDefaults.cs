namespace MVFC.Aspire.Helpers.Redis;

/// <summary>
/// Valores padrão para configuração do Redis.
/// </summary>
public static class RedisDefaults
{
    /// <summary>
    /// Imagem Docker padrão do Redis.
    /// </summary>
    public const string DefaultRedisImage = "redis";

    /// <summary>
    /// Tag padrão da imagem Docker do Redis.
    /// </summary>
    public const string DefaultRedisTag = "latest";

    /// <summary>
    /// Imagem Docker padrão do Redis Commander.
    /// </summary>
    public const string DefaultCommanderImage = "ghcr.io/joeferner/redis-commander";

    /// <summary>
    /// Tag padrão da imagem Docker do Redis Commander.
    /// </summary>
    public const string DefaultCommanderTag = "latest";

    /// <summary>
    /// Porta padrão do Redis.
    /// </summary>
    public const int DefaultRedisPort = 6379;

    /// <summary>
    /// Porta padrão do Redis Commander.
    /// </summary>
    public const int DefaultCommanderPort = 8081;

    /// <summary>
    /// Seção padrão da connection string do Redis na configuração da aplicação.
    /// </summary>
    public const string DefaultConnectionStringSection = "ConnectionStrings:redis";
}
