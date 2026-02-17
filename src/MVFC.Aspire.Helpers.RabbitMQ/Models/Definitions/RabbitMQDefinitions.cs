namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQDefinitions(
    [property: JsonPropertyName("users")] List<RabbitMQUser> Users,
    [property: JsonPropertyName("vhosts")] List<RabbitMQVhost> Vhosts,
    [property: JsonPropertyName("permissions")] List<RabbitMQPermission> Permissions,
    [property: JsonPropertyName("exchanges")] List<RabbitMQExchange> Exchanges,
    [property: JsonPropertyName("queues")] List<RabbitMQQueue> Queues,
    [property: JsonPropertyName("bindings")] List<RabbitMQBinding> Bindings);
