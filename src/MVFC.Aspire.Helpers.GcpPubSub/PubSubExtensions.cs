namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Fornece métodos de extensão para configurar, inicializar e integrar o emulador do Google Pub/Sub
/// em aplicações distribuídas, incluindo criação automática de tópicos, assinaturas e interface de administração via container.
/// </summary>
public static class PubSubExtensions {
    private const int HOST_PORT = 8681;
    private const int UI_PORT = 8680;
    private const int ACK_DEADLINE_SECONDS_DEFAULT = 300;
    private const int MAX_DELIVERY_ATTEMPTS_DEFAULT = 5;
    private const int WAIT_TIMEOUT_SECONDS_DEFAULT = 15;
    private const char CREATION_DELIMITER = ',';
    private const char SUBSCRIPTION_DELIMITER = ':';
    private const char DOCKER_IMAGE_DELIMITER = ':';

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns>Instância de <see cref="PubSubEmulatorResources"/> contendo os recursos do emulador e UI configurados.</returns>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        string name,
        IList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var emulatorConfig = new EmulatorConfig(name);
        var pubsubEmulator = builder.BuildPubSubEmulator(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);
        var pubsubUI = builder.BuildPubSubUI(emulatorConfig, pubsubEmulator, pubSubConfigs);

        return new PubSubEmulatorResources(pubsubEmulator, pubsubUI, pubSubConfigs);
    }

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="emulatorConfig">Configuração do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns>Instância de <see cref="PubSubEmulatorResources"/> contendo os recursos do emulador e UI configurados.</returns>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var pubsubEmulator = builder.BuildPubSubEmulator(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);
        var pubsubUI = builder.BuildPubSubUI(emulatorConfig, pubsubEmulator, pubSubConfigs);

        return new PubSubEmulatorResources(pubsubEmulator, pubsubUI, pubSubConfigs);
    }

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="emulatorConfig">Configuração do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns>Instância de <see cref="PubSubEmulatorResources"/> contendo os recursos do emulador e UI configurados.</returns>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) =>

        builder.AddGcpPubSub(emulatorConfig, [pubSubConfig], waitTimeoutSeconds);

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns>Instância de <see cref="PubSubEmulatorResources"/> contendo os recursos do emulador e UI configurados.</returns>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        string name,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) =>

        builder.AddGcpPubSub(new EmulatorConfig(name), [pubSubConfig], waitTimeoutSeconds);

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do emulador Pub/Sub e da interface de administração,
    /// define as variáveis de ambiente necessárias e prepara o ambiente Pub/Sub.
    /// </summary>
    /// <param name="project">Recurso do projeto que depende do Pub/Sub.</param>
    /// <param name="pubSubEmulatorResources">Recursos do emulador Pub/Sub e UI.</param>
    /// <returns><see cref="IResourceBuilder{ProjectResource}"/> configurado para aguardar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WaitForGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        PubSubEmulatorResources pubSubEmulatorResources) {

        project.WaitFor(pubSubEmulatorResources.Emulator)
               .WaitFor(pubSubEmulatorResources.UI)
               .AddGCPProjectsIds(pubSubEmulatorResources.PubSubConfigs)
               .WithEnvironment("PUBSUB_EMULATOR_HOST", $"localhost:{HOST_PORT.ToString()}")
               .PreparePubSubEnvironment(pubSubEmulatorResources);

        return project;
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto, configurando dependências e ambiente.
    /// </summary>
    /// <param name="project">Recurso do projeto que utilizará o Pub/Sub.</param>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns><see cref="IResourceBuilder{ProjectResource}"/> configurado para utilizar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        IList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var pubSub = builder.AddGcpPubSub(name, pubSubConfigs, waitTimeoutSeconds);

        return project.WaitForGcpPubSub(pubSub);
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto, configurando dependências e ambiente.
    /// </summary>
    /// <param name="project">Recurso do projeto que utilizará o Pub/Sub.</param>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="emulatorConfig">Configuração do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns><see cref="IResourceBuilder{ProjectResource}"/> configurado para utilizar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var pubSub = builder.AddGcpPubSub(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);

        return project.WaitForGcpPubSub(pubSub);
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto, configurando dependências e ambiente.
    /// </summary>
    /// <param name="project">Recurso do projeto que utilizará o Pub/Sub.</param>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns><see cref="IResourceBuilder{ProjectResource}"/> configurado para utilizar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var pubSub = builder.AddGcpPubSub(name, pubSubConfig, waitTimeoutSeconds);

        return project.WaitForGcpPubSub(pubSub);
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto, configurando dependências e ambiente.
    /// </summary>
    /// <param name="project">Recurso do projeto que utilizará o Pub/Sub.</param>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="emulatorConfig">Configuração do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId, tópicos e assinaturas.</param>
    /// <param name="waitTimeoutSeconds">
    /// (Opcional) Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// O valor padrão é 15 segundos.
    /// </param>
    /// <returns><see cref="IResourceBuilder{ProjectResource}"/> configurado para utilizar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = WAIT_TIMEOUT_SECONDS_DEFAULT) {

        var pubSub = builder.AddGcpPubSub(emulatorConfig, pubSubConfig, waitTimeoutSeconds);

        return project.WaitForGcpPubSub(pubSub);
    }

    /// <summary>
    /// Cria e configura o container da interface de administração do Pub/Sub Emulator.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="pubSubEmulator">Recurso do container do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub.</param>
    /// <returns><see cref="IResourceBuilder{ContainerResource}"/> configurado para a interface de administração.</returns>
    private static IResourceBuilder<ContainerResource> BuildPubSubUI(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IResourceBuilder<ContainerResource> pubSubEmulator,
        IList<PubSubConfig> pubSubConfigs) {

        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiName, nameof(emulatorConfig.UiName));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiTag, nameof(emulatorConfig.UiTag));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiImage, nameof(emulatorConfig.UiImage));

        return builder
            .AddContainer(emulatorConfig.UiName, BuildDockerImage(emulatorConfig.UiImage, emulatorConfig.UiTag))
            .WithEnvironment("PUBSUB_EMULATOR_HOST", $"host.docker.internal:{HOST_PORT.ToString()}")
            .AddGCPProjectsIds(pubSubConfigs)
            .WithHttpEndpoint(UI_PORT, UI_PORT, "http", isProxied: false)
            .WithHttpHealthCheck("/")
            .WaitFor(pubSubEmulator);
    }

    /// <summary>
    /// Adiciona a variável de ambiente <c>GCP_PROJECT_IDS</c> ao recurso informado, contendo uma lista separada por vírgula
    /// com os <c>ProjectId</c> de cada configuração Pub/Sub fornecida.
    /// </summary>
    /// <typeparam name="T">Tipo do recurso que implementa <see cref="IResourceWithEnvironment"/>.</typeparam>
    /// <param name="resource">Builder do recurso ao qual será adicionada a variável de ambiente.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub, cada uma contendo um <c>ProjectId</c>.</param>
    /// <returns>O builder do recurso atualizado com a variável de ambiente <c>GCP_PROJECT_IDS</c> configurada.</returns>
    private static IResourceBuilder<T> AddGCPProjectsIds<T>(
        this IResourceBuilder<T> resource,
        IList<PubSubConfig> pubSubConfigs)
        where T : IResourceWithEnvironment =>

        resource.WithEnvironment("GCP_PROJECT_IDS", string.Join(CREATION_DELIMITER, pubSubConfigs.Select(c => c.ProjectId)));

    /// <summary>
    /// Cria e configura o container do emulador do Google Pub/Sub.
    /// </summary>
    /// <param name="builder">Construtor da aplicação distribuída.</param>
    /// <param name="emulatorConfig">Configuração do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub.</param>
    /// <param name="waitTimeoutSeconds">
    /// Tempo máximo de espera, em segundos, para a inicialização completa do emulador Pub/Sub.
    /// Recomenda-se utilizar o valor padrão de 15 segundos, exceto em cenários específicos.
    /// </param>
    /// <returns><see cref="IResourceBuilder{ContainerResource}"/> configurado para o emulador Pub/Sub.</returns>
    private static IResourceBuilder<ContainerResource> BuildPubSubEmulator(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds) {

        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorName, nameof(emulatorConfig.EmulatorName));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorTag, nameof(emulatorConfig.EmulatorTag));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorImage, nameof(emulatorConfig.EmulatorImage));

        return builder.AddContainer(emulatorConfig.EmulatorName, BuildDockerImage(emulatorConfig.EmulatorImage, emulatorConfig.EmulatorTag))
                      .AddProjects(pubSubConfigs)
                      .WithEnvironment("PUBSUB_EMULATOR_WAIT_TIMEOUT", waitTimeoutSeconds.ToString())
                      .WithHttpEndpoint(HOST_PORT, HOST_PORT, "http", isProxied: false)
                      .WithHttpHealthCheck("/");
    }

    /// <summary>
    /// Constrói o nome completo da imagem Docker utilizando o nome da imagem e a tag informada.
    /// </summary>
    /// <param name="image">Nome da imagem Docker.</param>
    /// <param name="tag">Tag da imagem Docker.</param>
    /// <returns>String no formato "image:tag" para uso em containers.</returns>
    private static string BuildDockerImage(string image, string tag) =>
        new StringBuilder(image)
                .Append(DOCKER_IMAGE_DELIMITER)
                .Append(tag)
                .ToString();

    /// <summary>
    /// Adiciona variáveis de ambiente para cada projeto Pub/Sub configurado.
    /// </summary>
    /// <param name="container">Builder do container do emulador Pub/Sub.</param>
    /// <param name="pubSubConfigs">Lista de configurações do Pub/Sub.</param>
    /// <returns>O builder do container atualizado.</returns>
    private static IResourceBuilder<ContainerResource> AddProjects(
        this IResourceBuilder<ContainerResource> container,
        IList<PubSubConfig> pubSubConfigs) {

        var projectNumber = 0;

        foreach (var pubSubConfig in pubSubConfigs) container.WithEnvironment($"PUBSUB_PROJECT{++projectNumber}", BuildProjectId(pubSubConfig));

        return container;
    }

    /// <summary>
    /// Prepara o ambiente Pub/Sub, criando tópicos e assinaturas conforme a configuração fornecida.
    /// </summary>
    /// <param name="project">Builder do recurso do projeto que depende do Pub/Sub.</param>
    /// <param name="pubSubEmulatorResources">Recursos do emulador Pub/Sub e UI.</param>
    /// <returns>O builder do projeto configurado para preparar o ambiente Pub/Sub.</returns>
    private static IResourceBuilder<ProjectResource> PreparePubSubEnvironment(
        this IResourceBuilder<ProjectResource> project,
        PubSubEmulatorResources pubSubEmulatorResources) =>

        project.OnResourceReady(async (contexto, _, ct) => {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", $"localhost:{HOST_PORT}");

            var portEndpoint = contexto.GetEndpoint("http").Port;

            foreach (var pubSubConfig in pubSubEmulatorResources.PubSubConfigs) {
                await pubSubConfig.ConfigurePubSubAsync(portEndpoint, ct);
            }
        });

    /// <summary>
    /// Cria todos os tópicos e assinaturas definidos na configuração do Pub/Sub.
    /// </summary>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e lista de <see cref="MessageConfig"/>.</param>
    /// <param name="portEndpoint">Porta HTTP do emulador Pub/Sub.</param>
    /// <param name="ct">Token de cancelamento para operações assíncronas.</param>
    private static async Task ConfigurePubSubAsync(
        this PubSubConfig pubSubConfig,
        int portEndpoint,
        CancellationToken ct) {

        var pushEndpoint = $"http://host.docker.internal:{portEndpoint}";

        foreach (var messageConfig in pubSubConfig.MessageConfigs)
            await ModifyPushEndpoint(pubSubConfig.ProjectId, messageConfig, pushEndpoint, ct);
    }

    /// <summary>
    /// Modifica o endpoint de push de uma assinatura Pub/Sub no emulador.
    /// </summary>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <param name="messageConfig">Configuração da mensagem, incluindo nome do tópico, assinatura e endpoint de push.</param>
    /// <param name="pushEndpoint">Endpoint HTTP para receber mensagens push.</param>
    /// <param name="ct">Token de cancelamento para operações assíncronas.</param>
    public static async Task ModifyPushEndpoint(
        string projectId,
        MessageConfig messageConfig,
        string pushEndpoint,
        CancellationToken ct) {

        var subscriber = new SubscriberServiceApiClientBuilder() {
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build();

        try {
            var subscriptionName = SubscriptionName.FormatProjectSubscription(projectId, messageConfig.SubscriptionName);
            var subscription = subscriber.GetSubscription(subscriptionName);

            subscription.PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint);
            subscription.AckDeadlineSeconds = DefineAckDeadline(messageConfig);
            subscription.DeadLetterPolicy = BuildDeadLetterPolicy(projectId, messageConfig);

            await subscriber.UpdateSubscriptionAsync(subscription, BuildFieldMaskUpdate(messageConfig), ct);
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Cria a política de dead letter (DLQ) para a assinatura, se um tópico de dead letter for especificado.
    /// </summary>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <param name="messageConfig">Configuração da mensagem, incluindo o tópico de dead letter e o número máximo de tentativas.</param>
    /// <returns>
    /// Instância de <see cref="DeadLetterPolicy"/> configurada, ou <c>null</c> se não houver tópico de dead letter.
    /// </returns>
    private static DeadLetterPolicy? BuildDeadLetterPolicy(string projectId, MessageConfig messageConfig) {

        if (string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic)) return null;

        return new DeadLetterPolicy {
            DeadLetterTopic = TopicName.FormatProjectTopic(projectId, messageConfig.DeadLetterTopic),
            MaxDeliveryAttempts = messageConfig.MaxDeliveryAttempts ?? MAX_DELIVERY_ATTEMPTS_DEFAULT
        };
    }

    /// <summary>
    /// Define o tempo limite (deadline) para confirmação (ack) de mensagens na assinatura.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, podendo conter o tempo de deadline em segundos.</param>
    /// <returns>Tempo limite em segundos para confirmação de mensagens.</returns>
    private static int DefineAckDeadline(MessageConfig messageConfig) =>
        messageConfig.AckDeadlineSeconds ?? ACK_DEADLINE_SECONDS_DEFAULT;

    /// <summary>
    /// Cria o objeto <see cref="FieldMask"/> para atualização dos campos da assinatura.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, incluindo DeadLetterTopic e MaxDeliveryAttempts.</param>
    /// <returns>Objeto <see cref="FieldMask"/> com os campos "ack_deadline_seconds", "push_config", "dead_letter_policy".</returns>
    private static FieldMask BuildFieldMaskUpdate(MessageConfig messageConfig) {
        var paths = new List<string> { "ack_deadline_seconds", };

        if (!string.IsNullOrEmpty(messageConfig.PushEndpoint))
            paths.Add("push_config");

        if (!string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic))
            paths.Add("dead_letter_policy");

        return new FieldMask { Paths = { paths } };
    }

    /// <summary>
    /// Constrói a string de identificação do projeto e suas configurações de tópicos/assinaturas.
    /// </summary>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e lista de <see cref="MessageConfig"/>.</param>
    /// <returns>
    /// String contendo o ProjectId e os pares tópico:assinatura, separados por vírgula. Inclui também configurações de dead letter, se houver.
    /// </returns>
    private static string BuildProjectId(PubSubConfig pubSubConfig) {
        ArgumentException.ThrowIfNullOrWhiteSpace(pubSubConfig.ProjectId, nameof(pubSubConfig.ProjectId));

        var sb = new StringBuilder(pubSubConfig.ProjectId);

        if (!pubSubConfig.MessageConfigs.Any())
            return sb.ToString();

        return sb.Append(CREATION_DELIMITER)
                 .AppendJoin(CREATION_DELIMITER, pubSubConfig.MessageConfigs.Select(m => BuildTopicSubscription(m) + BuildDeadLetter(m.DeadLetterTopic)))
                 .ToString();
    }

    /// <summary>
    /// Constrói a string de identificação de um par tópico/assinatura.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, incluindo nome do tópico e da assinatura.</param>
    /// <returns>
    /// String no formato "topicName:subscriptionName".
    /// </returns>
    private static string BuildTopicSubscription(MessageConfig messageConfig) =>
        BuildTopicSubscription(messageConfig.TopicName, messageConfig.SubscriptionName);

    /// <summary>
    /// Constrói a string de identificação de um par tópico/assinatura.
    /// </summary>
    /// <param name="topicName">Nome do tópico Pub/Sub.</param>
    /// <param name="subscriptionName">Nome da assinatura associada ao tópico.</param>
    /// <returns>
    /// String no formato "topicName:subscriptionName".
    /// </returns>
    private static string BuildTopicSubscription(string topicName, string? subscriptionName) {
        var sb = new StringBuilder(topicName);

        if (!string.IsNullOrWhiteSpace(subscriptionName))
            sb.Append(SUBSCRIPTION_DELIMITER).Append(subscriptionName);

        return sb.ToString();
    }

    /// <summary>
    /// Constrói a string de identificação de dead letter para um tópico, se especificado.
    /// </summary>
    /// <param name="deadLetterTopic">Nome do tópico de dead letter (DLQ), ou <c>null</c> se não houver.</param>
    /// <returns>
    /// String no formato ",deadLetterTopic:deadLetterTopic-subscription" ou string vazia se não houver dead letter.
    /// </returns>
    private static string BuildDeadLetter(string? deadLetterTopic) {

        if (string.IsNullOrWhiteSpace(deadLetterTopic)) return string.Empty;

        var subscriptionName = $"{deadLetterTopic}-subscription";

        return $"{CREATION_DELIMITER}{BuildTopicSubscription(deadLetterTopic, subscriptionName)}";
    }

    /// <summary>
    /// Constrói a configuração de push para a assinatura, se necessário.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, incluindo endpoint de push.</param>
    /// <param name="pushEndpoint">Endpoint base para receber mensagens push.</param>
    /// <returns>Objeto <see cref="PushConfig"/> configurado ou null se não houver endpoint de push.</returns>
    private static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint) {

        if (string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)) return null;

        var fullPushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}";

        return new PushConfig() {
            PushEndpoint = fullPushEndpoint
        };
    }
}