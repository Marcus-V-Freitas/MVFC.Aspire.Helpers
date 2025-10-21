namespace MVFC.Aspire.Helpers.PubSub;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração, inicialização e integração do emulador do Google Pub/Sub
/// em aplicações distribuídas, incluindo a criação automática de tópicos, assinaturas e interface de administração via container.
/// </summary>
public static class PubSubExtensions {
    private const int HOST_PORT = 8681;
    private const int UI_PORT = 8680;
    private const string EMULATOR_IMAGE = "messagebird/gcloud-pubsub-emulator:latest";
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

        var pubsubEmulator = builder.BuildPubSubEmulator(name, pubSubConfig);
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
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="pubSubEmulator">O recurso do container do emulador Pub/Sub.</param>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <returns>O <see cref="IResourceBuilder{ContainerResource}"/> configurado para a interface de administração.</returns>
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
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso do emulador Pub/Sub.</param>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e tópicos/assinaturas.</param>
    /// <returns>O <see cref="IResourceBuilder{ContainerResource}"/> configurado para o emulador Pub/Sub.</returns>
    private static IResourceBuilder<ContainerResource> BuildPubSubEmulator(this IDistributedApplicationBuilder builder, string name, PubSubConfig pubSubConfig) =>
        builder
            .AddContainer(name, EMULATOR_IMAGE)
            .WithEnvironment("PUBSUB_PROJECT1", BuildProjectId(pubSubConfig))
            .WithHttpEndpoint(HOST_PORT, HOST_PORT, "http", isProxied: false)
            .WithHttpHealthCheck("/");

    /// <summary>
    /// Prepara o ambiente Pub/Sub, criando tópicos e assinaturas conforme a configuração fornecida.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá depender do Pub/Sub.</param>
    /// <param name="pubSubEmulatorResources">Recursos do emulador Pub/Sub e UI.</param>
    /// <returns>O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para preparar o ambiente Pub/Sub.</returns>
    private static IResourceBuilder<ProjectResource> PreparePubSubEnvironment(this IResourceBuilder<ProjectResource> project, PubSubEmulatorResources pubSubEmulatorResources) =>
        project.OnResourceReady(async (contexto, _, ct) => {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", $"localhost:{HOST_PORT}");

            var portEndpoint = contexto.GetEndpoint("http").Port;

            await pubSubEmulatorResources.PubSubConfig.ConfigurePubSubAsync(portEndpoint, ct);
        });

    /// <summary>
    /// Cria todos os tópicos e assinaturas definidos na configuração do Pub/Sub.
    /// </summary>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e lista de <see cref="MessageConfig"/>.</param>
    /// <param name="portEndpoint">Porta HTTP do emulador Pub/Sub.</param>
    /// <param name="ct">Token de cancelamento para operações assíncronas.</param>
    private static async Task ConfigurePubSubAsync(this PubSubConfig pubSubConfig, int portEndpoint, CancellationToken ct) {
        if (pubSubConfig == null)
            return;

        var pushEndpoint = $"http://host.docker.internal:{portEndpoint}";

        foreach (var messageConfig in pubSubConfig.MessageConfigs) {
            await ModifyPushEndpoint(pubSubConfig.ProjectId, messageConfig, pushEndpoint, ct);
        }
    }

    /// <summary>
    /// Modifica o endpoint de push de uma assinatura Pub/Sub no emulador.
    /// </summary>
    /// <param name="projectId">ID do projeto GCP utilizado pelo Pub/Sub.</param>
    /// <param name="messageConfig">Configuração da mensagem, incluindo nome do tópico, assinatura e endpoint de push.</param>
    /// <param name="pushEndpoint">Endpoint HTTP para receber mensagens push.</param>
    /// <param name="ct">Token de cancelamento para operações assíncronas.</param>
    public static async Task ModifyPushEndpoint(string projectId, MessageConfig messageConfig, string pushEndpoint, CancellationToken ct) {
        var subscriber = new SubscriberServiceApiClientBuilder() {
            EmulatorDetection = EmulatorDetection.EmulatorOnly
        }.Build();

        try {
            var subscription = subscriber.GetSubscription($"projects/{projectId}/subscriptions/{messageConfig.SubscriptionName}");

            subscription.PushConfig = BuildPushEndpoint(messageConfig, pushEndpoint);
            subscription.AckDeadlineSeconds = DefineAckDeadline(messageConfig);

            await subscriber.UpdateSubscriptionAsync(subscription, BuildFieldMaskUpdate(), ct);
        }
        catch (Exception ex) {

            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Define o tempo limite (deadline) para confirmação (ack) de mensagens na assinatura.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, podendo conter o tempo de deadline em segundos.</param>
    /// <returns>Tempo limite em segundos para confirmação de mensagens.</returns>
    private static int DefineAckDeadline(MessageConfig messageConfig) =>
        (messageConfig.AckDeadlineSeconds ?? TimeSpan.FromMinutes(5)).Seconds;

    /// <summary>
    /// Cria o objeto <see cref="FieldMask"/> para atualização dos campos da assinatura.
    /// </summary>
    /// <returns>Objeto <see cref="FieldMask"/> com os campos "ack_deadline_seconds" e "push_config".</returns>
    private static FieldMask BuildFieldMaskUpdate() =>
        new() {
            Paths =
            {
                "ack_deadline_seconds",
                "push_config"
            }
        };

    /// <summary>
    /// Constrói a string de identificação do projeto e suas configurações de tópicos/assinaturas.
    /// </summary>
    /// <param name="pubSubConfig">Configuração do Pub/Sub, incluindo ProjectId e lista de <see cref="MessageConfig"/>.</param>
    /// <returns>String contendo o ProjectId e os pares tópico:assinatura separados por vírgula.</returns>
    private static string BuildProjectId(PubSubConfig pubSubConfig) {
        var sb = new StringBuilder(pubSubConfig.ProjectId);

        if (!pubSubConfig.MessageConfigs.Any())
            return sb.ToString();

        return sb.Append(',')
                 .AppendJoin(',', pubSubConfig.MessageConfigs.Select(m => $"{m.TopicName}:{m.SubscriptionName}"))
                 .ToString();
    }

    /// <summary>
    /// Constrói a configuração de push para a assinatura, se necessário.
    /// </summary>
    /// <param name="messageConfig">Configuração da mensagem, incluindo endpoint de push.</param>
    /// <param name="pushEndpoint">Endpoint base para receber mensagens push.</param>
    /// <returns>Objeto <see cref="PushConfig"/> configurado ou null se não houver endpoint de push.</returns>
    private static PushConfig? BuildPushEndpoint(MessageConfig messageConfig, string pushEndpoint) {
        if (string.IsNullOrWhiteSpace(messageConfig.PushEndpoint)) {
            return null;
        }

        var fullPushEndpoint = $"{pushEndpoint.TrimEnd('/')}/{messageConfig.PushEndpoint.TrimStart('/')}";

        return new PushConfig() {
            PushEndpoint = fullPushEndpoint
        };
    }
}