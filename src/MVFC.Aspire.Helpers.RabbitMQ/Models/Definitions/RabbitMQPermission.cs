namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQPermission(
    [property: JsonPropertyName("user")] string User,
    [property: JsonPropertyName("vhost")] string Vhost,
    [property: JsonPropertyName("configure")] string Configure,
    [property: JsonPropertyName("write")] string Write,
    [property: JsonPropertyName("read")] string Read);
