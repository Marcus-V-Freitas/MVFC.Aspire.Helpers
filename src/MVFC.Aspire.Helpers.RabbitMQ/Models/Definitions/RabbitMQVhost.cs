namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQVhost(
    [property: JsonPropertyName("name")] string Name);
