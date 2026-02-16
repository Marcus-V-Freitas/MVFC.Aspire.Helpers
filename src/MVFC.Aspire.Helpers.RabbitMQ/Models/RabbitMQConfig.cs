namespace MVFC.Aspire.Helpers.RabbitMQ.Models;

/// <summary>
/// Configurações para o recurso RabbitMQ.
/// </summary>
/// <param name="Port">Porta AMQP do RabbitMQ (padrão: null para porta aleatória)</param>
/// <param name="ManagementPort">Porta do Management UI (padrão: null para porta aleatória)</param>
/// <param name="Username">Usuário do RabbitMQ</param>
/// <param name="Password">Senha do RabbitMQ</param>
/// <param name="Exchanges">Lista de exchanges a serem criados</param>
/// <param name="Queues">Lista de queues a serem criadas</param>
/// <param name="ImageName">Nome da imagem Docker do RabbitMQ</param>
/// <param name="ImageTag">Tag da imagem Docker do RabbitMQ</param>
/// <param name="VolumeName">Nome do volume para persistência de dados</param>
public sealed record RabbitMQConfig(
    int? Port = null,
    int? ManagementPort = null,
    string Username = RabbitMQDefaults.DefaultUsername,
    string Password = RabbitMQDefaults.DefaultPassword,
    IReadOnlyList<ExchangeConfig>? Exchanges = null,
    IReadOnlyList<QueueConfig>? Queues = null,
    string ImageName = RabbitMQDefaults.DefaultRabbitMQImage,
    string ImageTag = RabbitMQDefaults.DefaultRabbitMQTag,
    string? VolumeName = null);
