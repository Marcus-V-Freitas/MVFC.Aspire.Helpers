namespace MVFC.Aspire.Helpers.CloudStorage.Resources;

/// <summary>
/// Representa um recurso de Cloud Storage (emulador GCS) para uso em aplicações distribuídas,
/// fornecendo acesso ao endpoint HTTP e expressão de conexão para integração.
/// </summary>
public sealed class CloudStorageResource(string name)
    : ContainerResource(name), IResourceWithConnectionString {

    /// <summary>
    /// Nome padrão do endpoint HTTP exposto pelo recurso de Cloud Storage.
    /// </summary>
    internal const string HttpEndpointName = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Obtém a referência ao endpoint HTTP do recurso de Cloud Storage.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);

    /// <summary>
    /// Expressão que constrói a string de conexão para o endpoint do emulador de Cloud Storage,
    /// incluindo o esquema, host, porta e o sufixo "/storage/v1/".
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{HttpEndpoint.Property(EndpointProperty.Scheme)}://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}/storage/v1/"
        );
}