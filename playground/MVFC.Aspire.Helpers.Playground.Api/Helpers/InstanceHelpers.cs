namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

public static class InstanceHelpers
{
    public static async Task<PublisherServiceApiClient> CreatePubSubClientAsync() =>
        await new PublisherServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync().ConfigureAwait(false);

    public static async Task<StorageClient> CreateStorageClientAsync() =>
        await new StorageClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        }.BuildAsync().ConfigureAwait(false);

    public static SmtpClient CreateSmtp(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var smtpUri = new Uri(builder.Configuration.GetConnectionString("mailpit")!);
        return new SmtpClient(smtpUri.Host, smtpUri.Port);
    }

    public static async Task<IConnectionMultiplexer> CreateRedisAsync(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("redis")!).ConfigureAwait(false);
    }

    public static async Task<IConnection> CreateRabbitAsync(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

        var factory = new ConnectionFactory
        {
            Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!)
        };
        return await factory.CreateConnectionAsync().ConfigureAwait(false);
    }

    public static void AddGotenberg(this IServiceCollection services) =>
        services.AddRefitClient<IGotenbergApi>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var url = sp.GetRequiredService<IConfiguration>()["GOTENBERG:BASE_URL"]!;
                    client.BaseAddress = new Uri(url);
                });

    public static void AddKeycloak(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakBaseUrl = configuration["Keycloak:BaseUrl"]!;
        var keycloakRealm = configuration["Keycloak:Realm"]!;
        var keycloakClient = configuration["Keycloak:ClientId"]!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt =>
                {
                    jwt.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
                    jwt.Audience = keycloakClient;
                    jwt.RequireHttpsMetadata = false;
                    jwt.TokenValidationParameters = new()
                    {
                        ValidateAudience = true,
                        NameClaimType = "preferred_username",
                    };
                });

        services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", p => p.RequireRole("admin"))
                .AddPolicy("UserOrAbove", p => p.RequireRole("admin", "user"));
    }
}
