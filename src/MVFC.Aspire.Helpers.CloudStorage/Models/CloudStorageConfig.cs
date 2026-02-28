namespace MVFC.Aspire.Helpers.CloudStorage.Models;

/// <summary>
/// Configurações para o recurso de Cloud Storage (emulador GCS).
/// </summary>
/// <param name="LocalBucketFolder">Caminho local para a pasta de persistência dos buckets (montada no container; se nulo, armazenamento é volátil).</param>
/// <param name="Port">Porta do host para expor o serviço HTTP do emulador (se nulo, o Aspire aloca uma porta dinâmica).</param>
/// <param name="EmulatorImage">Nome da imagem Docker utilizada para o emulador CloudStorage.</param>
/// <param name="EmulatorTag">Tag da imagem Docker utilizada para o emulador Cloud Storage.</param>
public sealed record CloudStorageConfig(
    string? LocalBucketFolder = null,
    int? Port = null,
    string EmulatorImage = "fsouza/fake-gcs-server",
    string EmulatorTag = "latest");
