namespace MVFC.Aspire.Helpers.Gotenberg;

/// <summary>
/// Valores padrão para configuração do Gotenberg.
/// </summary>
public static class GotenbergDefaults
{
    /// <summary>Imagem Docker padrão do Gotenberg.</summary>
    public const string DEFAULT_IMAGE = "gotenberg/gotenberg";

    /// <summary>Tag padrão da imagem Docker do Gotenberg.</summary>
    /// <remarks>
    /// Use a linha principal 8.x, conforme documentação oficial.
    /// </remarks>
    public const string DEFAULT_IMAGE_TAG = "8";

    /// <summary>Porta HTTP padrão do Gotenberg dentro do container.</summary>
    public const int DEFAULT_HTTP_PORT = 3000;

    /// <summary>
    /// Caminho do endpoint de health check HTTP padrão do Gotenberg.
    /// </summary>
    /// <remarks>
    /// A documentação indica /health como endpoint de status.[web:28]
    /// </remarks>
    public const string HEALTH_PATH = "/health";

    /// <summary>
    /// Nome da variável de ambiente usada para expor a base URL do Gotenberg para a aplicação.
    /// </summary>
    /// <remarks>
    /// Ex: GOTENBERG__BASE_URL=http://gotenberg:3000
    /// </remarks>
    public const string BASE_URL_ENV_VAR = "GOTENBERG__BASE_URL";
}
