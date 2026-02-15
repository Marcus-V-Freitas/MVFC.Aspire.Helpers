namespace MVFC.Aspire.Helpers.GcpPubSub.Models;

/// <summary>
/// Representa os recursos do emulador do Google Pub/Sub utilizados em um ambiente de desenvolvimento distribuído,
/// incluindo o container do emulador, a interface de administração (UI) e as configurações do Pub/Sub.
/// </summary>
/// <param name="Emulator">
/// Recurso do container que executa o emulador do Google Pub/Sub.
/// </param>
/// <param name="UI">
/// Recurso do container responsável pela interface de administração do emulador Pub/Sub.
/// </param>
/// <param name="PubSubConfigs">
/// Lista de configurações do Pub/Sub, incluindo o ID do projeto, tópicos, assinaturas e delay de inicialização.
/// </param>
public sealed record class PubSubEmulatorResources(
    IResourceBuilder<ContainerResource> Emulator,
    IResourceBuilder<ContainerResource> UI,
    IReadOnlyList<PubSubConfig> PubSubConfigs);