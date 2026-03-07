namespace MVFC.Aspire.Helpers.RabbitMQ.Resources;

/// <summary>
/// Represents the RabbitMQ resource as an Aspire container, providing AMQP and Management endpoints
/// and a connection string expression for integration in distributed applications.
/// </summary>
public sealed class RabbitMQResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    /// <summary>
    /// AMQP endpoint name used by RabbitMQ.
    /// </summary>
    internal const string AMQP_ENDPOINT_NAME = "amqp";

    /// <summary>
    /// HTTP endpoint name for the RabbitMQ Management UI.
    /// </summary>
    internal const string MANAGEMENT_ENDPOINT_NAME = "management";

    private EndpointReference? _amqpReference;

    /// <summary>
    /// Reference to the AMQP endpoint of the RabbitMQ resource.
    /// </summary>
    public EndpointReference AmqpEndpoint =>
        _amqpReference ??= new(this, AMQP_ENDPOINT_NAME);

    /// <summary>
    /// Expression representing the AMQP connection string for RabbitMQ.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"amqp://{AmqpEndpoint.Property(EndpointProperty.Host)}:{AmqpEndpoint.Property(EndpointProperty.Port)}"
        );

    /// <summary>
    /// RabbitMQ username (used to generate definitions.json).
    /// </summary>
    internal string Username { get; set; } = RabbitMQDefaults.DEFAULT_USERNAME;

    /// <summary>
    /// RabbitMQ password (used to generate definitions.json).
    /// </summary>
    internal string Password { get; set; } = RabbitMQDefaults.DEFAULT_PASSWORD;

    /// <summary>
    /// List of configured exchanges.
    /// </summary>
    internal List<ExchangeConfig>? Exchanges { get; set; }

    /// <summary>
    /// List of configured queues.
    /// </summary>
    internal List<QueueConfig>? Queues { get; set; }
}
