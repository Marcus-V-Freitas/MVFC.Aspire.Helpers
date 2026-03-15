namespace MVFC.Aspire.Helpers.Tests.Integration.HttpClients;

internal interface IPlaygroundApiClient
{
    // Gotenberg
    [Post("/api/pdf")]
    [Headers("Content-Type: application/json")]
    internal Task<ApiResponse<Stream>> ConvertToPdfAsync([Body] JsonNode requestBody);

    // Mongo
    [Get("/api/mongo")]
    internal Task<ApiResponse<string>> GetMongoStatusAsync();

    // Cloud Storage
    [Get("/api/bucket/{bucketName}")]
    internal Task<ApiResponse<string>> GetCloudStorageBucketAsync(string bucketName);

    // PubSub
    [Get("/api/pub-sub-enter")]
    internal Task<ApiResponse<string>> GetPubSubEnterAsync();

    // MailPit
    [Post("/api/send-email")]
    [Headers("Content-Type: application/json")]
    internal Task<ApiResponse<string>> SendEmailAsync([Body] JsonNode requestBody);

    // Redis
    [Get("/api/redis/set/{key}/{value}")]
    internal Task<ApiResponse<string>> SetRedisValueAsync(string key, string value);

    [Get("/api/redis/get/{key}")]
    internal Task<ApiResponse<string>> GetRedisValueAsync(string key);

    // RabbitMQ
    [Post("/api/rabbitmq/publish/{exchange}/{routingKey}/{message}")]
    internal Task<ApiResponse<string>> PublishRabbitMqMessageAsync(string exchange, string routingKey, string message);

    [Get("/api/rabbitmq/consume/{queue}")]
    internal Task<ApiResponse<string>> ConsumeRabbitMqMessageAsync(string queue);

    // Keycloak
    [Get("/api/key-cloak")]
    internal Task<ApiResponse<string>> GetSecretDataAsync();

    [Get("/api/key-cloak")]
    internal Task<ApiResponse<string>> GetSecretDataWithTokenAsync([Authorize("Bearer")] string token);
}
