namespace MVFC.Aspire.Helpers.Mongo.Helpers;

/// <summary>
/// Helper para construção de nomes completos de imagens Docker
/// </summary>
internal static class DockerImageHelper {
    private const char Delimiter = ':';

    public static string Build(string image, string tag) {
        ArgumentException.ThrowIfNullOrWhiteSpace(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return $"{image}{Delimiter}{tag}";
    }
}
