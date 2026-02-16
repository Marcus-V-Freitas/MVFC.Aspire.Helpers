namespace MVFC.Aspire.Helpers.RabbitMQ.Models;

/// <summary>
/// Configuração de um Exchange do RabbitMQ.
/// </summary>
/// <param name="Name">Nome do exchange</param>
/// <param name="Type">Tipo do exchange (direct, topic, fanout, headers)</param>
/// <param name="Durable">Se o exchange deve ser durável</param>
/// <param name="AutoDelete">Se o exchange deve ser deletado automaticamente quando não usado</param>
public sealed record ExchangeConfig(
    string Name,
    string Type = "direct",
    bool Durable = true,
    bool AutoDelete = false);
