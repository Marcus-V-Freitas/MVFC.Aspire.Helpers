namespace MVFC.Aspire.Helpers.RabbitMQ.Models;

/// <summary>
/// RabbitMQ queue configuration.
/// </summary>
/// <param name="Name">Queue name</param>
/// <param name="ExchangeName">Name of the exchange to bind the queue to</param>
/// <param name="RoutingKey">Routing key for binding (default: queue name)</param>
/// <param name="Durable">Whether the queue should be durable</param>
/// <param name="AutoDelete">Whether the queue should be auto-deleted when unused</param>
/// <param name="DeadLetterExchange">Dead letter exchange name</param>
/// <param name="MessageTTL">Message time-to-live in milliseconds</param>
public sealed record QueueConfig(
    string Name,
    string? ExchangeName = null,
    string? RoutingKey = null,
    bool Durable = true,
    bool AutoDelete = false,
    string? DeadLetterExchange = null,
    int? MessageTTL = null);
