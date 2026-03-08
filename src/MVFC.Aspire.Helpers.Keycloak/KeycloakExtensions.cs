namespace MVFC.Aspire.Helpers.Keycloak;

public static class KeycloakExtensions
{
    public static IResourceBuilder<KeycloakResource> AddKeycloak(
        this IDistributedApplicationBuilder builder,
        string name,
        int httpPort = KeycloakDefaults.HOST_PORT,
        int managementPort = KeycloakDefaults.MANAGEMENT_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new KeycloakResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(KeycloakDefaults.DEFAULT_IMAGE, KeycloakDefaults.DEFAULT_TAG)
            .WithArgs(KeycloakDefaults.START_DEV_ARG)
            .WithEnvironment(KeycloakDefaults.ADMIN_USERNAME_ENV, resource.AdminUsername)
            .WithEnvironment(KeycloakDefaults.ADMIN_PASSWORD_ENV, resource.AdminPassword)
            .WithKeycloakEndpoint(httpPort, managementPort);
    }

    public static IResourceBuilder<KeycloakResource> WithDockerImage(
        this IResourceBuilder<KeycloakResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(image);
        ArgumentNullException.ThrowIfNullOrEmpty(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    public static IResourceBuilder<KeycloakResource> WithAdminCredentials(
        this IResourceBuilder<KeycloakResource> builder,
        string username,
        string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.AdminUsername = username;
        builder.Resource.AdminPassword = password;

        return builder
            .WithEnvironment(KeycloakDefaults.ADMIN_USERNAME_ENV, username)
            .WithEnvironment(KeycloakDefaults.ADMIN_PASSWORD_ENV, password);
    }

    public static IResourceBuilder<KeycloakResource> WithSeeds(
        this IResourceBuilder<KeycloakResource> builder,
        IReadOnlyCollection<IKeycloakRealmSeed> seeds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(seeds);

        builder.Resource.Seeds = seeds;

        if (seeds.Count == 0)
            return builder;

        builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
        {
            var realmFiles = seeds.Select(seed => new ContainerFile
            {
                Name = $"{seed.RealmName}-realm.json",
                Contents = KeycloakRealmJsonBuilder.Build(seed),
                Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                           UnixFileMode.GroupRead | UnixFileMode.OtherRead,
            }).ToArray();

            builder.WithContainerFiles(KeycloakDefaults.IMPORT_PATH, realmFiles)
                   .WithArgs(KeycloakDefaults.IMPORT_REALM_ARG);

            return Task.CompletedTask;
        });

        return builder;
    }

    public static IResourceBuilder<KeycloakResource> WithImportStrategy(
        this IResourceBuilder<KeycloakResource> builder,
        KeycloakImportStrategy strategy = KeycloakImportStrategy.IgnoreExisting)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var strategyValue = strategy switch
        {
            KeycloakImportStrategy.IgnoreExisting => "IGNORE_EXISTING",
            KeycloakImportStrategy.OverwriteExisting => "OVERWRITE_EXISTING",
            _ => throw new ArgumentOutOfRangeException(nameof(strategy)),
        };

        return builder.WithArgs($"--spi-import-importer-file-strategy={strategyValue}");
    }

    public static IResourceBuilder<KeycloakResource> WithDataVolume(
        this IResourceBuilder<KeycloakResource> builder,
        string volumeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(volumeName);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(volumeName, KeycloakDefaults.DATA_VOLUME_PATH);
    }

    /// <summary>
    /// Injeta BaseUrl, Realm, ClientId e ClientSecret como variáveis de ambiente no projeto.
    /// Na API, use AddKeycloakAuthentication(configuration) para consumir.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<KeycloakResource> keycloak,
        string realmName,
        string clientId,
        string? clientSecret = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(keycloak);
        ArgumentException.ThrowIfNullOrWhiteSpace(realmName);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        project.WithReference(source: keycloak)
               .WithEnvironment(KeycloakDefaults.BASE_URL_ENV, keycloak.Resource.ConnectionStringExpression)
               .WithEnvironment(KeycloakDefaults.REALM_ENV, realmName)
               .WithEnvironment(KeycloakDefaults.CLIENT_ID_ENV, clientId);

        if (clientSecret is not null)
            project.WithEnvironment(KeycloakDefaults.CLIENT_SECRET_ENV, clientSecret);

        return project;
    }

    private static IResourceBuilder<KeycloakResource> WithKeycloakEndpoint(
        this IResourceBuilder<KeycloakResource> resource,
        int httpPort,
        int managementPort)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(httpPort, IPEndPoint.MinPort);
        ArgumentOutOfRangeException.ThrowIfLessThan(managementPort, IPEndPoint.MinPort);

        return resource
            .WithHttpEndpoint(
                port: httpPort,
                targetPort: KeycloakDefaults.HOST_PORT,
                name: KeycloakResource.HTTP_ENDPOINT_NAME,
                isProxied: false)
            .WithHttpEndpoint(
                port: managementPort,
                targetPort: KeycloakDefaults.MANAGEMENT_PORT,
                name: KeycloakDefaults.MANAGEMENT_ENDPOINT,
                isProxied: false)
            .WithEnvironment(KeycloakDefaults.MANAGEMENT_ENV, "true")
            .WithHttpHealthCheck(
                path: KeycloakDefaults.HEALTH_PATH,
                endpointName: KeycloakDefaults.MANAGEMENT_ENDPOINT);
    }
}
