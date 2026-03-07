namespace MVFC.Aspire.Helpers.Gotenberg.Resources;

/// <summary>
/// Representa o recurso Gotenberg como um container Aspire, expondo um endpoint HTTP
/// e uma expressão de base URL para integração em aplicações distribuídas.
/// </summary>
/// <remarks>
/// Este recurso encapsula a configuração de endpoint necessária para o Gotenberg,
/// permitindo referência simples a partir de outros recursos do Aspire.
/// </remarks>
public sealed class GotenbergResource(string name)
    : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>
    /// Nome do endpoint HTTP usado pelo Gotenberg.
    /// </summary>
    internal const string HTTP_ENDPOINT_NAME = "http";

    private EndpointReference? _httpReference;

    /// <summary>
    /// Referência ao endpoint HTTP do recurso Gotenberg.
    /// </summary>
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HTTP_ENDPOINT_NAME);

    /// <summary>
    /// Expressão representando a URL base HTTP do Gotenberg.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}"
        );
}
