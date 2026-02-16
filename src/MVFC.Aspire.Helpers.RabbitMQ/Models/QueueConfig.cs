namespace MVFC.Aspire.Helpers.RabbitMQ.Models;

/// <summary>
/// Configuração de uma Queue do RabbitMQ.
/// </summary>
/// <param name="Name">Nome da queue</param>
/// <param name="ExchangeName">Nome do exchange ao qual a queue será vinculada</param>
/// <param name="RoutingKey">Routing key para binding (padrão: nome da queue)</param>
/// <param name="Durable">Se a queue deve ser durável</param>
/// <param name="AutoDelete">Se a queue deve ser deletada automaticamente quando não usada</param>
/// <param name="DeadLetterExchange">Nome do exchange de dead letter</param>
/// <param name="MessageTTL">Tempo de vida das mensagens em milissegundos</param>
public sealed record QueueConfig(
    string Name,
    string? ExchangeName = null,
    string? RoutingKey = null,
    bool Durable = true,
    bool AutoDelete = false,
    string? DeadLetterExchange = null,
    int? MessageTTL = null);
