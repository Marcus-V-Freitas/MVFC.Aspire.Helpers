namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQQueue(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vhost")] string Vhost,
    [property: JsonPropertyName("durable")] bool Durable,
    [property: JsonPropertyName("auto_delete")] bool AutoDelete,
    [property: JsonPropertyName("arguments")] Dictionary<string, object> Arguments);
