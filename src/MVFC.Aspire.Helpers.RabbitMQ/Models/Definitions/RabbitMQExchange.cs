namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQExchange(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vhost")] string Vhost,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("durable")] bool Durable,
    [property: JsonPropertyName("auto_delete")] bool AutoDelete,
    [property: JsonPropertyName("internal")] bool Internal,
    [property: JsonPropertyName("arguments")] Dictionary<string, object> Arguments);
