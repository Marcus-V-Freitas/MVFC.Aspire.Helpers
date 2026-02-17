namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQBinding(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("vhost")] string Vhost,
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("destination_type")] string DestinationType,
    [property: JsonPropertyName("routing_key")] string RoutingKey,
    [property: JsonPropertyName("arguments")] Dictionary<string, object> Arguments);
