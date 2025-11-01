namespace MVFC.Aspire.Helpers.Mailpit.Resources;

/// <summary>
/// Representa o recurso Mailpit como um container Aspire, fornecendo endpoints SMTP e HTTP
/// e uma expressão de string de conexão para integração em aplicações distribuídas.
/// </summary>
/// <remarks>
/// Esta classe encapsula a configuração dos endpoints necessários para o funcionamento do Mailpit,
/// permitindo fácil referência e integração com outros recursos Aspire.
/// </remarks>
public sealed class MailpitResource(string name) : ContainerResource(name), IResourceWithConnectionString {
    /// <summary>
    /// Nome do endpoint SMTP utilizado pelo Mailpit.
    /// </summary>
    internal const string SmtpEndpointName = "smtp";

    /// <summary>
    /// Nome do endpoint HTTP utilizado pelo Mailpit.
    /// </summary>
    internal const string HttpEndpointName = "http";

    private EndpointReference? _smtpReference;

    /// <summary>
    /// Referência ao endpoint SMTP do recurso Mailpit.
    /// </summary>
    public EndpointReference SmtpEndpoint =>
        _smtpReference ??= new(this, SmtpEndpointName);

    /// <summary>
    /// Expressão que representa a string de conexão SMTP para o Mailpit.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"smtp://{SmtpEndpoint.Property(EndpointProperty.Host)}:{SmtpEndpoint.Property(EndpointProperty.Port)}"
        );
}