namespace MVFC.Aspire.Helpers.GcpPubSub;

/// <summary>
/// Fornece métodos de extensão para configurar, inicializar e integrar o emulador do Google Pub/Sub
/// em aplicações distribuídas.
/// </summary>
public static class PubSubExtensions {
    private static readonly Lazy<SubscriberServiceApiClient> _subscriberClient =
        new(() => new SubscriberServiceApiClientBuilder {
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build());

    private static readonly Lazy<ILogger> _logger =
        new(() => LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(PubSubExtensions)));

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(emulatorConfig);
        ArgumentNullException.ThrowIfNull(pubSubConfigs);

        var pubsubEmulator = builder.BuildPubSubEmulator(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);
        var pubsubUI = builder.BuildPubSubUI(emulatorConfig, pubsubEmulator, pubSubConfigs);

        return new PubSubEmulatorResources(pubsubEmulator, pubsubUI, pubSubConfigs);
    }

    /// <summary>
    /// Adiciona o emulador do Google Pub/Sub e sua interface de administração à aplicação distribuída.
    /// </summary>
    public static PubSubEmulatorResources AddGcpPubSub(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) =>
            builder.AddGcpPubSub(emulatorConfig, [pubSubConfig], waitTimeoutSeconds);

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do emulador Pub/Sub e da interface de administração.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WaitForGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        PubSubEmulatorResources pubSubEmulatorResources) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(pubSubEmulatorResources);

        project.WaitFor(pubSubEmulatorResources.Emulator)
               .WaitFor(pubSubEmulatorResources.UI)
               .AddGCPProjectsIds(pubSubEmulatorResources.PubSubConfigs)
               .WithEnvironment("PUBSUB_EMULATOR_HOST", $"localhost:" + PubSubDefaults.HostPort)
               .PreparePubSubEnvironment(pubSubEmulatorResources);

        return project;
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var emulatorConfig = new EmulatorConfig(name);

        if (!builder.TryGetPubSubResources(emulatorConfig, pubSubConfigs, out var pubSub)) {
            pubSub = builder.AddGcpPubSub(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);
        }

        return project.WaitForGcpPubSub(pubSub!);
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(builder);

        if (!builder.TryGetPubSubResources(emulatorConfig, pubSubConfigs, out var pubSub)) {
            pubSub = builder.AddGcpPubSub(emulatorConfig, pubSubConfigs, waitTimeoutSeconds);
        }

        return project.WaitForGcpPubSub(pubSub!);
    }

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) =>
            project.WithGcpPubSub(builder, name, [pubSubConfig], waitTimeoutSeconds);

    /// <summary>
    /// Adiciona e integra o emulador do Google Pub/Sub ao projeto.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithGcpPubSub(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        PubSubConfig pubSubConfig,
        int waitTimeoutSeconds = PubSubDefaults.WaitTimeoutSecondsDefault) =>
            project.WithGcpPubSub(builder, emulatorConfig, [pubSubConfig], waitTimeoutSeconds);

    /// <summary>
    /// Tenta obter e reutilizar os recursos do emulador Pub/Sub já existentes.
    /// </summary>
    private static bool TryGetPubSubResources(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        out PubSubEmulatorResources? resources) {
        var emulatorExists = builder.TryCreateResourceBuilder<ContainerResource>(
            emulatorConfig.EmulatorName, out var emulator);
        var uiExists = builder.TryCreateResourceBuilder<ContainerResource>(
            emulatorConfig.UiName, out var ui);

        if (emulatorExists && uiExists && emulator is not null && ui is not null) {
            resources = new PubSubEmulatorResources(emulator, ui, pubSubConfigs);
            return true;
        }

        resources = null;
        return false;
    }

    /// <summary>
    /// Cria e configura o container da interface de administração do Pub/Sub.
    /// </summary>
    private static IResourceBuilder<ContainerResource> BuildPubSubUI(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IResourceBuilder<ContainerResource> pubSubEmulator,
        IReadOnlyList<PubSubConfig> pubSubConfigs) {
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiName, nameof(emulatorConfig.UiName));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiTag, nameof(emulatorConfig.UiTag));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.UiImage, nameof(emulatorConfig.UiImage));

        var dockerImage = BuildDockerImage(emulatorConfig.UiImage, emulatorConfig.UiTag);

        return builder
            .AddContainer(emulatorConfig.UiName, dockerImage)
            .WithEnvironment("PUBSUB_EMULATOR_HOST", $"host.docker.internal:" + PubSubDefaults.HostPort)
            .AddGCPProjectsIds(pubSubConfigs)
            .WithHttpEndpoint(PubSubDefaults.UIPort, PubSubDefaults.UIPort, "http", isProxied: false)
            .WithHttpHealthCheck("/")
            .WaitFor(pubSubEmulator);
    }

    /// <summary>
    /// Adiciona a variável de ambiente GCP_PROJECT_IDS ao recurso.
    /// </summary>
    private static IResourceBuilder<T> AddGCPProjectsIds<T>(
        this IResourceBuilder<T> resource,
        IReadOnlyList<PubSubConfig> pubSubConfigs)
        where T : IResourceWithEnvironment {
        var projectIds = string.Join(PubSubDefaults.CreationDelimiter,
            pubSubConfigs.Select(c => c.ProjectId));

        return resource.WithEnvironment("GCP_PROJECT_IDS", projectIds);
    }

    /// <summary>
    /// Cria e configura o container do emulador do Google Pub/Sub.
    /// </summary>
    private static IResourceBuilder<ContainerResource> BuildPubSubEmulator(
        this IDistributedApplicationBuilder builder,
        EmulatorConfig emulatorConfig,
        IReadOnlyList<PubSubConfig> pubSubConfigs,
        int waitTimeoutSeconds) {
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorName, nameof(emulatorConfig.EmulatorName));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorTag, nameof(emulatorConfig.EmulatorTag));
        ArgumentException.ThrowIfNullOrWhiteSpace(emulatorConfig.EmulatorImage, nameof(emulatorConfig.EmulatorImage));

        var dockerImage = BuildDockerImage(emulatorConfig.EmulatorImage, emulatorConfig.EmulatorTag);

        return builder
            .AddContainer(emulatorConfig.EmulatorName, dockerImage)
            .AddProjects(pubSubConfigs)
            .WithEnvironment("PUBSUB_EMULATOR_WAIT_TIMEOUT", waitTimeoutSeconds.ToString())
            .WithHttpEndpoint(PubSubDefaults.HostPort, PubSubDefaults.HostPort, "http", isProxied: false)
            .WithHttpHealthCheck("/");
    }

    /// <summary>
    /// Constrói o nome completo da imagem Docker.
    /// </summary>
    private static string BuildDockerImage(string image, string tag) =>
        $"{image}{PubSubDefaults.DockerImageDelimiter}{tag}";

    /// <summary>
    /// Adiciona variáveis de ambiente para cada projeto Pub/Sub configurado.
    /// </summary>
    private static IResourceBuilder<ContainerResource> AddProjects(
        this IResourceBuilder<ContainerResource> container,
        IReadOnlyList<PubSubConfig> pubSubConfigs) {
        var projectNumber = 0;

        foreach (var pubSubConfig in pubSubConfigs) {
            container.WithEnvironment($"PUBSUB_PROJECT{++projectNumber}", BuildProjectId(pubSubConfig));
        }

        return container;
    }

    /// <summary>
    /// Prepara o ambiente Pub/Sub, criando tópicos e assinaturas.
    /// </summary>
    private static IResourceBuilder<ProjectResource> PreparePubSubEnvironment(
        this IResourceBuilder<ProjectResource> project,
        PubSubEmulatorResources pubSubEmulatorResources) =>
            project.OnResourceReady(async (context, _, ct) => {
                Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", $"localhost:{PubSubDefaults.HostPort}");

                var portEndpoint = context.GetEndpoint("http").Port;

                foreach (var pubSubConfig in pubSubEmulatorResources.PubSubConfigs) {
                    await pubSubConfig.ConfigurePubSubAsync(portEndpoint, ct);
                }
            });

    /// <summary>
    /// Cria todos os tópicos e assinaturas definidos na configuração do Pub/Sub.
    /// </summary>
    private static async Task ConfigurePubSubAsync(
        this PubSubConfig pubSubConfig,
        int portEndpoint,
        CancellationToken ct) {
        var pushEndpoint = $"http://host.docker.internal:{portEndpoint}";

        var tasks = pubSubConfig.MessageConfigs
            .Select(mc => ModifyPushEndpoint(pubSubConfig.ProjectId, mc, pushEndpoint, ct))
            .ToList();

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Modifica o endpoint de push de uma assinatura Pub/Sub no emulador.
    /// </summary>
    private static async Task ModifyPushEndpoint(
        string projectId,
        MessageConfig messageConfig,
        string pushEndpoint,
        CancellationToken ct) {
        try {
            var subscriptionName = SubscriptionName.FormatProjectSubscription(projectId, messageConfig.SubscriptionName);
            var subscription = await _subscriberClient.Value.GetSubscriptionAsync(subscriptionName);

            subscription.PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint);
            subscription.AckDeadlineSeconds = DefineAckDeadline(messageConfig);
            subscription.DeadLetterPolicy = BuildDeadLetterPolicy(projectId, messageConfig);

            await _subscriberClient.Value.UpdateSubscriptionAsync(subscription, BuildFieldMaskUpdate(messageConfig), ct);
        }
        catch (Exception ex) {
            _logger.Value.LogSubscriptionConfigFailed(ex, messageConfig.SubscriptionName ?? "unknown");
        }
    }

    /// <summary>
    /// Cria a política de dead letter (DLQ) para a assinatura.
    /// </summary>
    private static DeadLetterPolicy? BuildDeadLetterPolicy(string projectId, MessageConfig messageConfig) {
        if (string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic)) {
            return null;
        }

        return new DeadLetterPolicy {
            DeadLetterTopic = TopicName.FormatProjectTopic(projectId, messageConfig.DeadLetterTopic),
            MaxDeliveryAttempts = messageConfig.MaxDeliveryAttempts ?? PubSubDefaults.MaxDeliveryAttemptsDefault
        };
    }

    /// <summary>
    /// Define o tempo limite (deadline) para confirmação (ack) de mensagens.
    /// </summary>
    private static int DefineAckDeadline(MessageConfig messageConfig) =>
        messageConfig.AckDeadlineSeconds ?? PubSubDefaults.AckDeadlineSecondsDefault;

    /// <summary>
    /// Cria o objeto FieldMask para atualização dos campos da assinatura.
    /// </summary>
    private static FieldMask BuildFieldMaskUpdate(MessageConfig messageConfig) {
        var paths = new List<string> { "ack_deadline_seconds" };

        if (!string.IsNullOrEmpty(messageConfig.PushEndpoint)) {
            paths.Add("push_config");
        }

        if (!string.IsNullOrWhiteSpace(messageConfig.DeadLetterTopic)) {
            paths.Add("dead_letter_policy");
        }

        return new FieldMask { Paths = { paths } };
    }

    /// <summary>
    /// Constrói a string de identificação do projeto e suas configurações.
    /// </summary>
    private static string BuildProjectId(PubSubConfig pubSubConfig) {
        ArgumentException.ThrowIfNullOrWhiteSpace(pubSubConfig.ProjectId, nameof(pubSubConfig.ProjectId));

        var sb = new StringBuilder(pubSubConfig.ProjectId);

        if (!pubSubConfig.MessageConfigs.Any()) {
            return sb.ToString();
        }

        return sb.Append(PubSubDefaults.CreationDelimiter)
                 .AppendJoin(PubSubDefaults.CreationDelimiter,
                     pubSubConfig.MessageConfigs.Select(m => BuildTopicSubscription(m) + BuildDeadLetter(m.DeadLetterTopic)))
                 .ToString();
    }

    /// <summary>
    /// Constrói a string de identificação de um par tópico/assinatura.
    /// </summary>
    private static string BuildTopicSubscription(MessageConfig messageConfig) =>
        BuildTopicSubscription(messageConfig.TopicName, messageConfig.SubscriptionName);

    /// <summary>
    /// Constrói a string de identificação de um par tópico/assinatura.
    /// </summary>
    private static string BuildTopicSubscription(string topicName, string? subscriptionName) {
        var sb = new StringBuilder(topicName);

        if (!string.IsNullOrWhiteSpace(subscriptionName)) {
            sb.Append(PubSubDefaults.SubscriptionDelimiter).Append(subscriptionName);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Constrói a string de identificação de dead letter para um tópico.
    /// </summary>
    private static string BuildDeadLetter(string? deadLetterTopic) {
        if (string.IsNullOrWhiteSpace(deadLetterTopic)) {
            return string.Empty;
        }

        var subscriptionName = $"{deadLetterTopic}-subscription";

        return $"{PubSubDefaults.CreationDelimiter}{BuildTopicSubscription(deadLetterTopic, subscriptionName)}";
    }

    /// <summary>
    /// Constrói a configuração de push para a assinatura.
    /// </summary>
    private static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint) {
        if (string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)) {
            return null;
        }

        var fullPushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}";

        return new PushConfig {
            PushEndpoint = fullPushEndpoint
        };
    }
}
