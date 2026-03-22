namespace MVFC.Aspire.Helpers.Playground.Api.Helpers;

public static class InstanceHelpers
{
    public static SpannerConnection CreateSpannerConnection(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection("Spanner").Get<SpannerOptions>()
                      ?? throw new InvalidOperationException("Spanner configuration is missing.");

        var dataSource = DatabaseName.FormatProjectInstanceDatabase(
            options.ProjectId,
            options.InstanceId,
            options.DatabaseId);

        var builder = new SpannerConnectionStringBuilder()
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
            DataSource = dataSource,
        };

        return new SpannerConnection(builder.ConnectionString);
    }

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

        return new SmtpClient(smtpUri.Host, smtpUri.Port)
        {
            Credentials = new NetworkCredential("teste", "teste"),
        };
    }

    public static async Task<IConnectionMultiplexer> CreateRedisAsync(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("redis")!);
        options.Password = "teste";

        return await ConnectionMultiplexer.ConnectAsync(options).ConfigureAwait(false);
    }

    public static async Task<IConnection> CreateRabbitAsync(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

        var factory = new ConnectionFactory
        {
            Uri = new Uri(builder.Configuration.GetConnectionString("rabbitmq")!),
            Password = "teste",
            UserName = "teste",
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
        ArgumentNullException.ThrowIfNull(configuration);

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
