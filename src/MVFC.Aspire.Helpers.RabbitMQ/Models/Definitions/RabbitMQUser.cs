namespace MVFC.Aspire.Helpers.RabbitMQ.Models.Definitions;

internal sealed record RabbitMQUser(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("password_hash")] string PasswordHash,
    [property: JsonPropertyName("hashing_algorithm")] string HashingAlgorithm,
    [property: JsonPropertyName("tags")] string[] Tags);
