namespace MVFC.Aspire.Helpers.PubSub;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração, inicialização e integração do emulador do Google Pub/Sub
/// em aplicações distribuídas, incluindo a criação automática de tópicos, assinaturas e interface de administração via container.
/// </summary>
public static class PubSubExtensions {
    private const int HOST_PORT = 8085;
    private const int UI_PORT = 8680;
    private const string EMULATOR_IMAGE = "google/cloud-sdk:latest";
    private const string UI_IMAGE = "echocode/gcp-pubsub-emulator-ui:latest";

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e tópicos/assinaturas.</param>
    /// <returns>Um objeto <see cref="PubSubEmulatorResources"/> contendo os recursos do emulador e UI configurados.</returns>
    /// <exception cref="ArgumentException">Lançada se <paramref name="name"/> ou <paramref name="pubSubConfig.ProjectId"/> for nulo ou vazio.</exception>
    public static PubSubEmulatorResources AddGcpPubSub(this IDistributedApplicationBuilder builder, string name, PubSubConfig pubSubConfig) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(pubSubConfig.ProjectId, nameof(pubSubConfig.ProjectId));

        var pubsubEmulator = builder.BuildPubSubEmulator(name, pubSubConfig.ProjectId);
        var pubsubUI = builder.BuildPubSubUI(pubsubEmulator, pubSubConfig.ProjectId);

        return new PubSubEmulatorResources(pubsubEmulator, pubsubUI, pubSubConfig);
    }

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do emulador Pub/Sub e da interface de administração,
    /// além de definir as variáveis de ambiente necessárias e preparar o ambiente Pub/Sub.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá depender do Pub/Sub.</param>
    /// <param name="pubSubEmulatorResources">Recursos do emulador Pub/Sub e UI.</param>
    /// <returns>O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para aguardar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WaitForGcpPubSub(this IResourceBuilder<ProjectResource> project, PubSubEmulatorResources pubSubEmulatorResources) {
        project.WaitFor(pubSubEmulatorResources.Emulator)
               .WaitFor(pubSubEmulatorResources.UI)
               .WithEnvironment("GCP_PROJECT_IDS", pubSubEmulatorResources.PubSubConfig.ProjectId)
               .WithEnvironment("PUBSUB_EMULATOR_HOST", $"localhost:{HOST_PORT.ToString()}")
               .PreparePubSubEnvironment(pubSubEmulatorResources);

        return project;
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto, configurando dependências e ambiente.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o Pub/Sub.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e tópicos/assinaturas.</param>
    /// <returns>O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o Pub/Sub.</returns>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(this IResourceBuilder<ProjectResource> project, IDistributedApplicationBuilder builder, string name, PubSubConfig pubSubConfig) {
        var pubSub = builder.AddGcpPubSub(name, pubSubConfig);

        return project.WaitForGcpPubSub(pubSub);
    }

    /// <summary>
    /// Cria e configura o container da interface de administração do Pub/Sub Emulator.
    /// </summary>
    private static IResourceBuilder<ContainerResource> BuildPubSubUI(this IDistributedApplicationBuilder builder, IResourceBuilder<ContainerResource> pubSubEmulator, string projectId) =>
        builder
            .AddContainer("pubsub-ui", UI_IMAGE)
            .WithEnvironment("PUBSUB_EMULATOR_HOST", $"host.docker.internal:{HOST_PORT.ToString()}")
            .WithEnvironment("GCP_PROJECT_IDS", projectId)
            .WithHttpEndpoint(UI_PORT, UI_PORT, "http", isProxied: false)
            .WithHttpHealthCheck("/")
            .WaitFor(pubSubEmulator);

    /// <summary>
    /// Cria e configura o container do emulador do Google Pub/Sub.
    /// </summary>
    private static IResourceBuilder<ContainerResource> BuildPubSubEmulator(this IDistributedApplicationBuilder builder, string name, string projectId) =>
        builder
            .AddContainer(name, EMULATOR_IMAGE)
            .WithArgs(
                "gcloud", "beta", "emulators", "pubsub", "start",
                $"--host-port=0.0.0.0:{HOST_PORT}",
                $"--project={projectId}")
            .WithEnvironment("PUBSUB_PROJECT_ID", projectId)
            .WithHttpEndpoint(HOST_PORT, HOST_PORT, "http", isProxied: false)
            .WithHttpHealthCheck("/");

    /// <summary>
    /// Prepara o ambiente Pub/Sub, criando tópicos e assinaturas conforme a configuração fornecida.
    /// </summary>
    private static IResourceBuilder<ProjectResource> PreparePubSubEnvironment(this IResourceBuilder<ProjectResource> project, PubSubEmulatorResources pubSubEmulatorResources) =>
        project.OnResourceReady(async (contexto, _, ct) => {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", $"localhost:{HOST_PORT}");

            var portEndpoint = contexto.GetEndpoint("http").Port;

            await pubSubEmulatorResources.PubSubConfig.ConfigurePubSubAsync(portEndpoint, ct);
        });

    /// <summary>
    /// Cria todos os tópicos e assinaturas definidos na configuração do Pub/Sub.
    /// </summary>
    private static async Task ConfigurePubSubAsync(this PubSubConfig pubSubConfig, int portEndpoint, CancellationToken ct) {
        if (pubSubConfig == null)
            return;

        var pushEndpoint = $"http://host.docker.internal:{portEndpoint}";

        foreach (var messageConfig in pubSubConfig.MessageConfigs) {
            await pubSubConfig.ConfigurePubSubAsync(messageConfig, pushEndpoint, ct);
        }
    }

    /// <summary>
    /// Cria um tópico e uma assinatura para uma configuração de mensagem específica.
    /// </summary>
    private static async Task ConfigurePubSubAsync(this PubSubConfig pubSubConfig, MessageConfig messageConfig, string pushEndpoint, CancellationToken ct) {
        await Task.Delay(pubSubConfig.UpDelay, ct);

        var topicName = await pubSubConfig.CreateTopicAsync(messageConfig, ct);

        await pubSubConfig.CreateSubscriptionAsync(topicName, messageConfig, pushEndpoint, ct);
    }

    /// <summary>
    /// Constrói a configuração de push para a assinatura, se necessário.
    /// </summary>
    private static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint) {
        if (string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)) {
            return null;
        }

        var fullPushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}";

        return new PushConfig() {
            PushEndpoint = fullPushEndpoint
        };
    }

    /// <summary>
    /// Cria uma assinatura para um tópico Pub/Sub.
    /// </summary>
    private static async Task<Subscription> CreateSubscriptionAsync(this PubSubConfig pubSubConfig, TopicName topicName, MessageConfig messageConfig, string pushEndpoint, CancellationToken ct) {
        var subscriber = new SubscriberServiceApiClientBuilder() {
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build();

        var subscription = new Subscription() {
            SubscriptionName = SubscriptionName.FromProjectSubscription(pubSubConfig.ProjectId, messageConfig.SubscriptionName),
            TopicAsTopicName = topicName,
            AckDeadlineSeconds = TimeSpan.FromMinutes(5).Seconds,
            PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint),
        };

        try {
            await subscriber.CreateSubscriptionAsync(subscription, ct);
        }
        catch { }

        return subscription;
    }

    /// <summary>
    /// Cria um tópico Pub/Sub.
    /// </summary>
    private static async Task<TopicName> CreateTopicAsync(this PubSubConfig pubSubConfig, MessageConfig messageConfig, CancellationToken ct) {
        var publisher = new PublisherServiceApiClientBuilder() {
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build();

        var topicName = TopicName.FromProjectTopic(pubSubConfig.ProjectId, messageConfig.TopicName);

        try {
            await publisher.CreateTopicAsync(topicName, ct);
        }
        catch { }

        return topicName;
    }
}