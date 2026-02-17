namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

public static class InstanceHelpers {

    public static async Task<PublisherServiceApiClient> CreatePubSubClientAsync() =>
        await new PublisherServiceApiClientBuilder {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync();

    public static async Task<StorageClient> CreateStorageClientAsync() =>
        await new StorageClientBuilder {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync();

    public static SmtpClient CreateSmtp(this WebApplicationBuilder builder) {
        var smtpUri = new Uri(builder.Configuration.GetConnectionString("mailpit")!);
        return new SmtpClient(smtpUri.Host, smtpUri.Port);
    }

    public static async Task<IConnectionMultiplexer> CreateRedisAsync(this WebApplicationBuilder builder) => 
        await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("redis")!);

    public static async Task<IConnection> CreateRabbitAsync(this WebApplicationBuilder builder) {
        var factory = new ConnectionFactory {
            Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
        };
        return await factory.CreateConnectionAsync();
    }
}
