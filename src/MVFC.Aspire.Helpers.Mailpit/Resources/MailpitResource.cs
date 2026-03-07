namespace MVFC.Aspire.Helpers.Mailpit.Resources;

/// <summary>
/// Represents the Mailpit resource as an Aspire container, providing SMTP and HTTP endpoints
/// and a connection string expression for integration in distributed applications.
/// </summary>
/// <remarks>
/// This class encapsulates the endpoint configuration required for Mailpit operation,
/// enabling easy referencing and integration with other Aspire resources.
/// </remarks>
public sealed class MailpitResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>
    /// SMTP endpoint name used by Mailpit.
    /// </summary>
    internal const string SMTP_ENDPOINT_NAME = "smtp";

    /// <summary>
    /// HTTP endpoint name used by Mailpit.
    /// </summary>
    internal const string HTTP_ENDPOINT_NAME = "http";

    private EndpointReference? _smtpReference;

    /// <summary>
    /// Reference to the SMTP endpoint of the Mailpit resource.
    /// </summary>
    public EndpointReference SmtpEndpoint =>
        _smtpReference ??= new(this, SMTP_ENDPOINT_NAME);

    /// <summary>
    /// Expression representing the SMTP connection string for Mailpit.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"smtp://{SmtpEndpoint.Property(EndpointProperty.Host)}:{SmtpEndpoint.Property(EndpointProperty.Port)}"
        );
}
