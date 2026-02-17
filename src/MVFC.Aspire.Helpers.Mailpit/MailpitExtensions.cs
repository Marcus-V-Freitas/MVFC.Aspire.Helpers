namespace MVFC.Aspire.Helpers.Mailpit;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração do recurso Mailpit
/// em aplicações distribuídas utilizando o Aspire.
/// </summary>
public static class MailpitExtensions {

    /// <summary>
    /// Adiciona um recurso Mailpit à aplicação distribuída.
    /// Permite configurar opções como imagem, portas, autenticação e persistência de dados.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso Mailpit.</param>
    /// <param name="mailpitConfig">Configurações opcionais para o Mailpit. Se nulo, utiliza valores padrão.</param>
    /// <returns>Um <see cref="IResourceBuilder{MailpitResource}"/> configur
    public static IResourceBuilder<MailpitResource> AddMailpit(
        this IDistributedApplicationBuilder builder,
        string name,
        MailpitConfig? mailpitConfig = null) {

        mailpitConfig ??= new MailpitConfig();

        var resource = new MailpitResource(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(mailpitConfig.ImageName)
            .WithImageTag(mailpitConfig.ImageTag)
            .WithHttpEndpoint(
                port: mailpitConfig.HttpPort,
                targetPort: 8025,
                name: MailpitResource.HttpEndpointName)
            .WithEndpoint(
                port: mailpitConfig.SmtpPort,
                targetPort: 1025,
                name: MailpitResource.SmtpEndpointName);

        resourceBuilder.WithEnvironment("MP_MAX_MESSAGES", mailpitConfig.MaxMessages.ToString());

        if (mailpitConfig.MaxMessageSize > 0) {
            resourceBuilder.WithEnvironment("MP_MAX_MESSAGE_SIZE", (mailpitConfig.MaxMessageSize * 1024 * 1024).ToString());
        }

        if (mailpitConfig.SmtpAuthAcceptAny) {
            resourceBuilder.WithEnvironment("MP_SMTP_AUTH_ACCEPT_ANY", "1");
        }

        if (mailpitConfig.SmtpAuthAllowInsecure) {
            resourceBuilder.WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "1");
        }

        if (!string.IsNullOrWhiteSpace(mailpitConfig.SmtpHostname)) {
            resourceBuilder.WithEnvironment("MP_SMTP_HOSTNAME", mailpitConfig.SmtpHostname);
        }

        if (!string.IsNullOrWhiteSpace(mailpitConfig.DataFilePath)) {
            resourceBuilder
                .WithEnvironment("MP_DATA_FILE", "/data/mailpit.db")
                .WithBindMount(mailpitConfig.DataFilePath, "/data");
        }

        if (mailpitConfig.EnableWebAuth &&
            !string.IsNullOrWhiteSpace(mailpitConfig.WebAuthUsername) &&
            !string.IsNullOrWhiteSpace(mailpitConfig.WebAuthPassword)) {
            resourceBuilder
                .WithEnvironment("MP_UI_AUTH_USERNAME", mailpitConfig.WebAuthUsername)
                .WithEnvironment("MP_UI_AUTH_PASSWORD", mailpitConfig.WebAuthPassword);
        }

        if (mailpitConfig.VerboseLogging) {
            resourceBuilder.WithEnvironment("MP_VERBOSE", "1");
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Aguarda até que o recurso Mailpit esteja disponível antes de iniciar o projeto Aspire.
    /// Adiciona uma referência ao recurso Mailpit, garantindo que o projeto só será iniciado após o Mailpit estar pronto.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto Aspire.</param>
    /// <param name="mailpit">O builder do recurso Mailpit.</param>
    /// <returns>O builder do recurso do projeto Aspire com dependência do Mailpit.</returns>
    public static IResourceBuilder<ProjectResource> WaitForMailPit(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<MailpitResource> mailpit) =>

        project.WaitFor(mailpit)
               .WithReference(mailpit);

    /// <summary>
    /// Adiciona uma referência ao recurso Mailpit em um projeto Aspire.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso Mailpit.</param>
    /// <param name="mailpitConfig">Configurações opcionais para o Mailpit.</param>
    /// <returns>O builder do recurso do projeto com a referência ao Mailpit.</returns>
    public static IResourceBuilder<ProjectResource> WithMailPit(this IResourceBuilder<ProjectResource> project, IDistributedApplicationBuilder builder, string name, MailpitConfig? mailpitConfig = null) {
        IResourceBuilder<MailpitResource> mailpit;

        if (!builder.TryCreateResourceBuilder(name, out mailpit!)) {
            mailpit = builder.AddMailpit(name, mailpitConfig);
        }

        return project.WaitFor(mailpit)
                      .WithReference(mailpit);
    }
}
