namespace MVFC.Aspire.Helpers.RabbitMQ.Models;

/// <summary>
/// RabbitMQ exchange configuration.
/// </summary>
/// <param name="Name">Exchange name</param>
/// <param name="Type">Exchange type (direct, topic, fanout, headers)</param>
/// <param name="Durable">Whether the exchange should be durable</param>
/// <param name="AutoDelete">Whether the exchange should be auto-deleted when unused</param>
public sealed record ExchangeConfig(
    string Name,
    string Type = "direct",
    bool Durable = true,
    bool AutoDelete = false);
