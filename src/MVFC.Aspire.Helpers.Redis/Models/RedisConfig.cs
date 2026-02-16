namespace MVFC.Aspire.Helpers.Redis.Models;

/// <summary>
/// Configurações para o recurso Redis.
/// </summary>
/// <param name="Port">Porta do Redis (padrão: null para porta aleatória)</param>
/// <param name="Password">Senha para autenticação no Redis</param>
/// <param name="WithCommander">Incluir Redis Commander UI</param>
/// <param name="CommanderPort">Porta do Redis Commander (padrão: null para porta aleatória)</param>
/// <param name="VolumeName">Nome do volume para persistência de dados</param>
/// <param name="ImageName">Nome da imagem Docker do Redis</param>
/// <param name="ImageTag">Tag da imagem Docker do Redis</param>
/// <param name="CommanderImageName">Nome da imagem Docker do Redis Commander</param>
/// <param name="CommanderImageTag">Tag da imagem Docker do Redis Commander</param>
public sealed record RedisConfig(
    int? Port = null,
    string? Password = null,
    bool WithCommander = false,
    int? CommanderPort = null,
    string? VolumeName = null,
    string ImageName = RedisDefaults.DefaultRedisImage,
    string ImageTag = RedisDefaults.DefaultRedisTag,
    string CommanderImageName = RedisDefaults.DefaultCommanderImage,
    string CommanderImageTag = RedisDefaults.DefaultCommanderTag);
