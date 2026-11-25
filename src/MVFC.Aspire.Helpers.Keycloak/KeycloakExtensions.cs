namespace MVFC.Aspire.Helpers.Keycloak;

/// <summary>
/// Extension methods for configuring and referencing Keycloak resources in Aspire.
/// </summary>
public static class KeycloakExtensions
{
    /// <summary>
    /// Adds a Keycloak container with HTTP and management endpoints.
    /// </summary>
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

    /// <summary>
    /// Replaces the Keycloak container image and tag.
    /// </summary>
    public static IResourceBuilder<KeycloakResource> WithDockerImage(
        this IResourceBuilder<KeycloakResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Sets the administrator credentials used by the Keycloak container.
    /// </summary>
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

    /// <summary>
    /// Replaces the realm seeds read by the single callback before application startup.
    /// </summary>
    public static IResourceBuilder<KeycloakResource> WithSeeds(
        this IResourceBuilder<KeycloakResource> builder,
        params IReadOnlyCollection<IKeycloakRealmSeed> seeds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(seeds);

        builder.Resource.Seeds = seeds;

        if (builder.Resource.TryGetAnnotationsOfType<KeycloakSeedsCallbackAnnotation>(out _))
            return builder;

        builder.WithAnnotation(new KeycloakSeedsCallbackAnnotation());
        builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
        {
            var currentSeeds = builder.Resource.Seeds ?? [];
            var realmFiles = currentSeeds.Select(seed => new ContainerFile
            {
                Name = $"{seed.RealmName}-realm.json",
                Contents = KeycloakRealmJsonBuilder.Build(seed),
                Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                           UnixFileMode.GroupRead | UnixFileMode.OtherRead,
            }).ToArray();

            if (realmFiles.Length > 0)
            {
                builder.WithContainerFiles(KeycloakDefaults.IMPORT_PATH, realmFiles)
                       .WithArgs(KeycloakDefaults.IMPORT_REALM_ARG);
            }

            return Task.CompletedTask;
        });

        return builder;
    }

    /// <summary>
    /// Selects whether realm imports preserve or overwrite existing realms.
    /// </summary>
    public static IResourceBuilder<KeycloakResource> WithImportStrategy(
        this IResourceBuilder<KeycloakResource> builder,
        KeycloakImportStrategy strategy = KeycloakImportStrategy.IgnoreExisting)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var strategyValue = strategy == KeycloakImportStrategy.IgnoreExisting ? KeycloakDefaults.IGNORE_EXISTING_IMPORT_STRATEGY : KeycloakDefaults.OVERWRITE_EXISTING_IMPORT_STRATEGY;

        return builder.WithArgs($"{KeycloakDefaults.IMPORT_STRATEGY_ARG}={strategyValue}");
    }

    /// <summary>
    /// Mounts a named volume for persistent Keycloak data.
    /// </summary>
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

    /// <summary>
    /// Configures proxied HTTP and management endpoints with a readiness health check.
    /// </summary>
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
                isProxied: true)
            .WithHttpEndpoint(
                port: managementPort,
                targetPort: KeycloakDefaults.MANAGEMENT_PORT,
                name: KeycloakDefaults.MANAGEMENT_ENDPOINT,
                isProxied: true)
            .WithEnvironment(KeycloakDefaults.MANAGEMENT_ENV, "true")
            .WithEnvironment(KeycloakDefaults.HTTP_PORT_ENV, KeycloakDefaults.HOST_PORT.ToString(CultureInfo.InvariantCulture))
            .WithHttpHealthCheck(
                path: KeycloakDefaults.HEALTH_PATH,
                endpointName: KeycloakDefaults.MANAGEMENT_ENDPOINT);
    }
}
