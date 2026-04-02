namespace MVFC.Aspire.Helpers.Mailpit;

/// <summary>
/// Provides extension methods to simplify the configuration and integration of the Mailpit resource
/// in distributed applications using Aspire.
/// </summary>
public static class MailpitExtensions
{
    /// <summary>
    /// Adds a Mailpit resource to the distributed application with default settings.
    /// Use fluent methods such as WithMaxMessages, WithWebAuth, WithVerboseLogging to customize.
    /// </summary>
    public static IResourceBuilder<MailpitResource> AddMailpit(
        this IDistributedApplicationBuilder builder,
        string name,
        int httpPort = MailpitDefaults.DEFAULT_HTTP_PORT,
        int smtpPort = MailpitDefaults.DEFAULT_SMTP_PORT)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(builder);

        var resource = new MailpitResource(name);

        return builder.AddResource(resource)
            .WithDockerImage(
                image: MailpitDefaults.DEFAULT_IMAGE,
                tag: MailpitDefaults.DEFAULT_IMAGE_TAG)
            .WithMailpitEndpoint(httpPort, smtpPort)
            .WithMaxMessages(MailpitDefaults.DEFAULT_MAX_MESSAGES)
            .WithSmtpAuth();
    }

    /// <summary>
    /// Replaces the Docker image used by the Mailpit resource.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithDockerImage(
        this IResourceBuilder<MailpitResource> builder,
        string image,
        string tag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(image);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithImage(image).WithImageTag(tag);
    }

    /// <summary>
    /// Sets the maximum number of stored messages.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithMaxMessages(
        this IResourceBuilder<MailpitResource> builder,
        int maxMessages)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxMessages, 0);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.MAX_MESSAGES_ENV_VAR, maxMessages.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Sets the message size limit in MB.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithMaxMessageSize(
        this IResourceBuilder<MailpitResource> builder,
        int sizeMb)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sizeMb, 0);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.MAX_MESSAGE_SIZE_ENV_VAR, (sizeMb * 1024 * 1024).ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Configures SMTP authentication (accept any credential and/or allow insecure).
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithSmtpAuth(
        this IResourceBuilder<MailpitResource> builder,
        bool acceptAny = true,
        bool allowInsecure = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.SMTP_AUTH_ACCEPT_ANY_ENV_VAR, acceptAny ? MailpitDefaults.TRUE_VALUE : MailpitDefaults.FALSE_VALUE)
                      .WithEnvironment(MailpitDefaults.SMTP_AUTH_ALLOW_INSECURE_ENV_VAR, allowInsecure ? MailpitDefaults.TRUE_VALUE : MailpitDefaults.FALSE_VALUE);
    }

    /// <summary>
    /// Sets the SMTP server hostname.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithSmtpHostname(
        this IResourceBuilder<MailpitResource> builder,
        string hostname)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(hostname);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.SMTP_HOSTNAME_ENV_VAR, hostname);
    }

    /// <summary>
    /// Configures Mailpit persistence with a bind mount to the specified path.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithDataFile(
        this IResourceBuilder<MailpitResource> builder,
        string dataFilePath)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(dataFilePath);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.DATA_FILE_ENV_VAR, MailpitDefaults.DATA_FILE_PATH)
                      .WithBindMount(dataFilePath, MailpitDefaults.VOLUME_MOUNT_PATH);
    }

    /// <summary>
    /// Enables basic authentication on the Mailpit web interface.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithWebAuth(
        this IResourceBuilder<MailpitResource> builder,
        string username,
        string password)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithEnvironment(MailpitDefaults.UI_AUTH_USERNAME_ENV_VAR, username)
                      .WithEnvironment(MailpitDefaults.UI_AUTH_PASSWORD_ENV_VAR, password);
    }

    /// <summary>
    /// Enables verbose logging mode.
    /// </summary>
    public static IResourceBuilder<MailpitResource> WithVerboseLogging(
        this IResourceBuilder<MailpitResource> builder,
        bool enableLogging = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithEnvironment(MailpitDefaults.VERBOSE_ENV_VAR, enableLogging ? MailpitDefaults.TRUE_VALUE : MailpitDefaults.FALSE_VALUE);
        return builder;
    }

    /// <summary>
    /// Adds a reference to the Mailpit resource in the project.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MailpitResource> mailpit)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(mailpit);

        return project.WithReference(source: mailpit);
    }

    /// <summary>
    /// Configures the Mailpit HTTP and SMTP endpoints, without proxy.
    /// </summary>
    private static IResourceBuilder<MailpitResource> WithMailpitEndpoint(
        this IResourceBuilder<MailpitResource> resource,
        int httpPort,
        int smtpPort)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(httpPort, IPEndPoint.MinPort);
        ArgumentOutOfRangeException.ThrowIfLessThan(smtpPort, IPEndPoint.MinPort);

        return resource
            .WithHttpEndpoint(
                port: httpPort,
                targetPort: MailpitDefaults.DEFAULT_HTTP_PORT,
                name: MailpitResource.HTTP_ENDPOINT_NAME,
                isProxied: false)
            .WithEndpoint(
                port: smtpPort,
                targetPort: MailpitDefaults.DEFAULT_SMTP_PORT,
                name: MailpitResource.SMTP_ENDPOINT_NAME,
                isProxied: false);
    }
}
