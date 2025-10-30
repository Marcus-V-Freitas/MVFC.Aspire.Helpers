namespace MVFC.Aspire.Helpers.GcpPubSub.Models;

/// <summary>
/// Representa a configuração do emulador do Google Pub/Sub, incluindo nomes, imagens e tags dos containers do emulador e da interface de administração.
/// </summary>
/// <param name="EmulatorName">Nome do recurso do emulador Pub/Sub.</param>
/// <param name="UiName">Nome do recurso da interface de administração do emulador Pub/Sub.</param>
/// <param name="EmulatorImage">Nome da imagem Docker utilizada para o emulador Pub/Sub.</param>
/// <param name="EmulatorTag">Tag da imagem Docker do emulador Pub/Sub.</param>
/// <param name="UiImage">Nome da imagem Docker utilizada para a interface de administração do emulador Pub/Sub.</param>
/// <param name="UiTag">Tag da imagem Docker da interface de administração.</param>
public sealed record class EmulatorConfig(
    string EmulatorName,
    string UiName = "pubsub-ui",
    string EmulatorImage = "thekevjames/gcloud-pubsub-emulator",
    string EmulatorTag = "latest",
    string UiImage = "echocode/gcp-pubsub-emulator-ui",
    string UiTag = "latest");