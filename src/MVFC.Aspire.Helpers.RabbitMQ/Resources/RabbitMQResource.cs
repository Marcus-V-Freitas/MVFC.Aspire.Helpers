namespace MVFC.Aspire.Helpers.RabbitMQ.Resources;

/// <summary>
/// Representa o recurso RabbitMQ como um container Aspire, fornecendo endpoints AMQP e Management
/// e uma expressão de string de conexão para integração em aplicações distribuídas.
/// </summary>
/// <remarks>
/// Esta classe encapsula a configuração dos endpoints necessários para o funcionamento do RabbitMQ,
/// permitindo fácil referência e integração com outros recursos Aspire.
/// </remarks>
public sealed class RabbitMQResource(string name) : ContainerResource(name), IResourceWithConnectionString {
    /// <summary>
    /// Nome do endpoint AMQP utilizado pelo RabbitMQ.
    /// </summary>
    internal const string AmqpEndpointName = "amqp";

    /// <summary>
    /// Nome do endpoint HTTP do Management UI utilizado pelo RabbitMQ.
    /// </summary>
    internal const string ManagementEndpointName = "management";

    private EndpointReference? _amqpReference;

    /// <summary>
    /// Referência ao endpoint AMQP do recurso RabbitMQ.
    /// </summary>
    public EndpointReference AmqpEndpoint =>
        _amqpReference ??= new(this, AmqpEndpointName);

    /// <summary>
    /// Expressão que representa a string de conexão AMQP para o RabbitMQ.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"amqp://{AmqpEndpoint.Property(EndpointProperty.Host)}:{AmqpEndpoint.Property(EndpointProperty.Port)}"
        );
}
